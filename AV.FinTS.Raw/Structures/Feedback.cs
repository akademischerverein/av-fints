using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class Feedback : IElementStructure
    {
        public int Code { get; set; }

        public string? Reference { get; set; } = null!;

        public string Text { get; set; } = null!;

        public List<string?> Parameters { get; set; } = new();

        public static Feedback Read(MessageReader reader)
        {
            reader.EnterGroup();
            var feedback = new Feedback
            {
                Code = (int)reader.ReadInt()!,
                Reference = reader.Read(),
                Text = reader.Read()!
            };
            while (!reader.GroupEnded)
            {
                feedback.Parameters.Add(reader.Read());
            }
            reader.LeaveGroup();
            return feedback;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
