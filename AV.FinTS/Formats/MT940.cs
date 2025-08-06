using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AV.FinTS.Formats
{
    internal class MT940
    {
        private static NumberFormatInfo CommaDecimal;
        private static Regex regexStructuredPurpose = new(@"\?\d{2}", RegexOptions.Compiled | RegexOptions.Multiline, TimeSpan.FromSeconds(2));
        static MT940()
        {
            CommaDecimal = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            CommaDecimal.NumberDecimalSeparator = ",";
        }

        internal static List<SwiftStatement> Parse(byte[] dataset)
        {
            var strDataset = Encoding.ASCII.GetString(dataset);
            var sets = strDataset.Split("\r\n-", StringSplitOptions.RemoveEmptyEntries).Select(s => s.Substring(s.IndexOf("\r\n") + 2)).Select(ParseMessage).ToList();
            return sets;
        }

        private static SwiftStatement ParseMessage(string msg)
        {
            var lines = msg.Split("\r\n");
            var currentTag = (string?)null;
            var currentLine = string.Empty;

            var processedLines = new List<KeyValuePair<string, string>>();

            foreach(var line in lines)
            {
                if (line[0] == ':')
                {
                    if (currentTag != null)
                    {
                        processedLines.Add(KeyValuePair.Create(currentTag, currentLine));
                    }

                    currentTag = line.Substring(1, line.IndexOf(':', 1) - 1);
                    currentLine = line.Substring(line.IndexOf(':', 1) + 1);
                } else
                {
                    currentLine += "\r\n" + line;
                }
            }
            if (currentTag != null)
            {
                processedLines.Add(KeyValuePair.Create(currentTag, currentLine));
            }

            var stmt = new SwiftStatement();
            var trans = (SwiftTransaction?)null;
            var endofStmt = false;

            foreach(var line in processedLines)
            {
                switch(line.Key)
                {
                    case "20":
                        stmt.OrderReference = line.Value;
                        break;

                    case "21":
                        stmt.Referencing = line.Value;
                        break;

                    case "25":
                        var bankAccVal = line.Value.Split("/");
                        stmt.BankCode = bankAccVal[0];
                        stmt.AccountCode = bankAccVal[1];
                        break;

                    case "60F":
                    case "60M":
                        var openingMul = 1;
                        if (line.Value[0] == 'D')
                        {
                            openingMul = -1;
                        }

                        var openingDate = line.Value.Substring(1, 6);
                        if (openingDate != "000000")
                        {
                            stmt.OpeningDate = DateOnly.ParseExact("20" + openingDate, "yyyyMMdd");
                        }
                        stmt.Currency = line.Value.Substring(7, 3);
                        stmt.OpeningBalance = decimal.Parse(line.Value.Substring(10)) * openingMul;
                        break;

                    case "61":
                        trans = new SwiftTransaction();
                        stmt.Transactions.Add(trans);
                        trans.Field61 = true;
                        trans.ValueDate = DateOnly.ParseExact("20" + line.Value.Substring(0, 6), "yyyyMMdd");
                        int Uoffset = 0;
                        if (char.IsDigit(line.Value[6]))
                        {
                            trans.BookingDate = DateOnly.ParseExact(trans.ValueDate.Year.ToString("D4") + line.Value.Substring(6 + Uoffset, 4), "yyyyMMdd");
                            Uoffset += 4;

                            if (trans.ValueDate.Month == 1 && trans.BookingDate.Value.Month == 12)
                            {
                                trans.BookingDate.Value.AddYears(-1);
                            } else if (trans.ValueDate.Month == 12 && trans.BookingDate.Value.Month == 1)
                            {
                                trans.BookingDate.Value.AddYears(1);
                            }
                        }
                        var transMul = 1;
                        if (line.Value[6 + Uoffset] == 'R')
                        {
                            // reversed with stornos
                            transMul = line.Value[7 + Uoffset] == 'D' ? 1 : -1;
                            Uoffset += 1;
                        } else
                        {
                            transMul = line.Value[6 + Uoffset] == 'D' ? -1 : 1;
                        }

                        if (!char.IsDigit(line.Value[7 + Uoffset]))
                        {
                            Uoffset += 1;
                        }
                        var transTypeStart = line.Value.IndexOf('N', 7 + Uoffset);
                        var decimalStr = line.Value.Substring(7 + Uoffset, transTypeStart - (7 + Uoffset));
                        trans.Amount = decimal.Parse(decimalStr, CommaDecimal) * transMul;
                        trans.TransactionType = line.Value.Substring(transTypeStart, 4);
                        var remaining = line.Value.Substring(transTypeStart + 4);
                        trans.CustomerReference = remaining;
                        if (remaining.Contains("//"))
                        {
                            trans.CustomerReference = remaining.Substring(0, remaining.IndexOf("//"));
                            remaining = remaining.Substring(remaining.IndexOf("//") + 2);
                            trans.BankReference = remaining;

                            if (remaining.Contains("\r\n"))
                            {
                                trans.BankReference = remaining.Substring(0, remaining.IndexOf("\r\n"));
                                trans.FurtherInformation = remaining.Substring(remaining.IndexOf("\r\n"));
                            }
                        }
                        break;

                    case "86":
                        if(endofStmt)
                        {
                            stmt.Text = line.Value;
                            break;
                        }
                        if (trans == null || trans.Field86)
                        {
                            trans = new SwiftTransaction();
                            stmt.Transactions.Add(trans);
                        }
                        trans.Field86 = true;

                        if(line.Value.Length < 3 || !line.Value.Substring(0, 3).All(char.IsDigit) || int.Parse(line.Value.Substring(0, 3)) >= 900)
                        {
                            trans.Unstructured = line.Value;
                            trans.IsStructured = false;
                            break;
                        }
                        trans.IsStructured = true;
                        trans.TypeCode = int.Parse(line.Value.Substring(0, 3));
                        var remainingPurp = line.Value.Substring(3);

                        while (remainingPurp.Length > 0)
                        {
                            Debug.Assert(remainingPurp[0] == '?');
                            var nextCodeMatch = regexStructuredPurpose.Match(remainingPurp, 1);
                            var nextCode = nextCodeMatch.Success ? nextCodeMatch.Index : remainingPurp.Length;
                            var nextLineBreak = remainingPurp.IndexOf("\r\n");
                            if (nextLineBreak == -1) nextLineBreak = remainingPurp.Length;
                            var curVal = remainingPurp.Substring(3, (nextLineBreak > nextCode ? nextCode : nextLineBreak) - 3);
                            switch (remainingPurp.Substring(1, 2))
                            {
                                case "00":
                                    trans.Text = curVal;
                                    break;

                                case "10":
                                    trans.Primanoto = curVal;
                                    break;

                                case "20":
                                case "21":
                                case "22":
                                case "23":
                                case "24":
                                case "25":
                                case "26":
                                case "27":
                                case "28":
                                case "29":
                                case "60":
                                case "61":
                                case "62":
                                case "63":
                                    if (trans.Purpose == null)
                                    {
                                        trans.Purpose = string.Empty;
                                    }
                                    trans.Purpose += curVal;
                                    break;

                                case "30":
                                    trans.PartnerBankCode = curVal;
                                    break;

                                case "31":
                                    trans.PartnerAccountCode = curVal;
                                    break;

                                case "32":
                                case "33":
                                    if (trans.PartnerName == null)
                                    {
                                        trans.PartnerName = string.Empty;
                                    }
                                    trans.PartnerName += curVal;
                                    break;

                                case "34":
                                    trans.TextKeyAddition = curVal;
                                    break;

                                default:
                                    throw new NotImplementedException("MT940: Unknown purpose subtag");
                            }

                            remainingPurp = remainingPurp.Substring(curVal.Length + 3);
                            if (remainingPurp.StartsWith("\r\n")) {
                                remainingPurp = remainingPurp.Substring(2);
                            }
                        }

                        break;

                    case "62F":
                    case "62M":
                        endofStmt = true;
                        if (stmt.Currency != line.Value.Substring(7, 3))
                        {
                            throw new NotSupportedException("changing currencies not supported");
                        }

                        var closingMul = 1;
                        if (line.Value[0] == 'D')
                        {
                            closingMul = -1;
                        }

                        var closingDate = line.Value.Substring(1, 6);
                        stmt.ClosingDate = DateOnly.ParseExact("20" + closingDate, "yyyyMMdd");
                        stmt.ClosingBalance = decimal.Parse(line.Value.Substring(10)) * closingMul;
                        break;

                    case "28C":
                        if (line.Value.Contains('/'))
                        {
                            var auszug = line.Value.Split("/");
                            stmt.Number = int.Parse(auszug[0]);
                            stmt.PageNumber = int.Parse(auszug[1]);
                        } else
                        {
                            stmt.Number = int.Parse(line.Value);
                        }
                        break;

                    // TODO: WIP
                    case "64": // Aktueller Vaulutensaldo
                    case "65": // Zukünftiger Valutensaldo
                        break;

                    default:
                        throw new NotImplementedException("MT940: Unknown tag " + line.Key);
                }
            }
            return stmt;
        }
    }

#pragma warning disable CS8618
    public class SwiftStatement
    {
        // Field 20 Auftragsreferenznummer
        public string OrderReference { get; set; }

        // Field 21 Bezugsreferenznummer
        public string? Referencing { get; set; }

        // Field 25 Kontobezeichnung
        // SubField 1
        public string BankCode { get; set; }

        // Field 25 Kontobezeichnung
        // SubField 2
        public string AccountCode { get; set; }

        // Field 28C Auszugsnummer
        // SubField 1 Auszugsnummer
        public int Number { get; set; }

        // Field 28C Auszugsnummer
        // SubField 2 Blattnummer
        public int? PageNumber { get; set; }

        public string Currency { get; set; }

        #region Field 60a Anfangssaldo
        // SubField 1 & 4 Soll/Haben-Kennung & Betrag
        public decimal OpeningBalance { get; set; }

        // SubField 2 Buchungsdatum
        public DateOnly? OpeningDate { get; set; }
        #endregion

        #region Field 62a Schlusssaldo
        // SubField 1 & 4 Soll/Haben-Kennung & Betrag
        public decimal ClosingBalance { get; set; }

        // SubField 2 Buchungsdatum
        public DateOnly ClosingDate { get; set; }
        #endregion

        public List<SwiftTransaction> Transactions { get; set; } = new();

        // Field 86 Mehrzweckfeld
        public string? Text { get; set; }
    }

    public class SwiftTransaction
    {
        public bool Field61 { get; set; } = false;
        public bool Field86 { get; set; } = false;

        #region Field 61 Umsatz
        // SubField 1 (Valuta)Datum
        public DateOnly ValueDate { get; set; }

        // SubField 2 Buchungsdatum
        public DateOnly? BookingDate { get; set; }

        // SubField 3 & 5 Soll/Haben-Kennung & Betrag
        public decimal Amount { get; set; }

        // SubField 6 Buchungsschlüssel
        public string TransactionType { get; set; }

        // SubField 7 Referenz
        public string CustomerReference { get; set; }

        // SubField 8 Bankreferenz
        public string? BankReference { get; set; }

        public string? FurtherInformation { get; set; }
        #endregion

        #region Field 86 Mehrzweckfeld
        public bool IsStructured { get; set; } = false;

        public string? Unstructured { get; set; }

        // Geschäftsvorfallcode
        public int TypeCode { get; set; }

        // Field 00 Buchungstext
        public string? Text { get; set; }

        // Field 10 Primanoten-Nr.
        public string? Primanoto { get; set; }

        // Field 20-29, 60-63 Verwendungszweck
        public string? Purpose { get; set; }

        // Field 30 BLZ od. BIC Überweisender / Zahlungsempfänger
        public string? PartnerBankCode { get; set; }

        // Field 31 Konto-Nr./IBAN Überweisender / Zahlungsempfänger
        public string? PartnerAccountCode { get; set; }

        // Field 32-33 Name Überweisender / Zahlungsempfänger
        public string? PartnerName { get; set; }

        // Field 34 Textschlüsselergänzung
        public string? TextKeyAddition { get; set; }
        #endregion
    }
#pragma warning restore CS8618
}
