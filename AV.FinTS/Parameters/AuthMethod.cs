using AV.FinTS.Raw.Segments.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Parameter
{
    public class AuthMethod
    {
        public string Name { get; private set; } = null!;

        internal int Process { get; private set; }

        internal int SecurityFunction { get; private set; }

        public string TechnicalName { get; private set; } = null!;

        public Spec? TechnicalSpecName { get; private set; } = null!;

        public string? TechnicalSpecVersion { get; private set; } = null!;

        public bool MediumRequired { get; private set; }

        public TanInfo? Tan { get; private set; } = null!;

        public DecoupledInfo? Decoupled { get; private set; } = null!;

        public bool HhdUcRequired { get; private set; }

        internal int Version { get; private set; }

        public bool Structured { get; private set; }

        public static AuthMethod CreateFrom(HITANS6.TwoStepParameters param)
        {
            Spec? spec = null;

            if (param.ZkaTanName != null)
            {
                if (Enum.TryParse<Spec>(param.ZkaTanName, true, out var specNonNull))
                {
                    spec = specNonNull;
                }
            }

            return new AuthMethod
            {
                Name = param.TwoStepProcedureName,
                Process = param.TanProcess,
                SecurityFunction = param.SecurityFunction,
                TechnicalName = param.TechnicalName,
                TechnicalSpecName = spec,
                TechnicalSpecVersion = param.ZkaTanVersion,
                MediumRequired = param.CountActiveTanMedia > 1 && param.TanMediumRequired == 2,
                Tan = new ()
                {
                    Name = param.ReturnValueName,
                    Format = param.AllowedTanFormat,
                    MaxLength = param.MaxTanLength,
                },
                HhdUcRequired = param.AnswerHhdUcRequired,
                Version = 6,
                Structured = param.ChallengeStructured
            };
        }

        public static AuthMethod CreateFrom(HITANS7.TwoStepParameters param)
        {
            Spec? spec = null;

            if (param.DkTanName != null)
            {
                if (Enum.TryParse<Spec>(param.DkTanName, true, out var specNonNull))
                {
                    spec = specNonNull;
                }
            }

            var auth = new AuthMethod
            {
                Name = param.TwoStepProcedureName,
                Process = param.TanProcess,
                SecurityFunction = param.SecurityFunction,
                TechnicalName = param.TechnicalName,
                TechnicalSpecName = spec,
                TechnicalSpecVersion = param.DkTanVersion,
                MediumRequired = param.CountActiveTanMedia > 1 && param.TanMediumRequired == 2,
                HhdUcRequired = param.AnswerHhdUcRequired,
                Version = 7,
                Structured = param.ChallengeStructured
            };

            if (spec != Spec.Decoupled && spec != Spec.DecoupledPush)
            {
                auth.Tan = new()
                {
                    Name = param.ReturnValueName,
                    Format = (int)param.AllowedTanFormat!,
                    MaxLength = (int)param.MaxTanLength!,
                };
            } else if (spec == Spec.Decoupled)
            {
                auth.Decoupled = new()
                {
                    MaximumStatusRequests = (int)param.MaximumStatusRequests!,
                    WaitTimeFirstRequest = (int)param.WaitTimeBeforeFirstRequest!,
                    WaitTimeFollowingRequest = (int)param.WaitTimeBeforeNextRequest!,
                    ManualConfirmationAllowed = (bool)param.ConfirmationAllowed!,
                    AutomaticStatusRequestAllowed = (bool)param.AutomaticStatusRequestsAllowed!
                };
            }

            return auth;
        }

        public enum Spec
        {
            HHD,
            HHDUC,
            HHDOPT1,
            Secoder_UC,
            HHDUSB1, // spec unknown
            mobileTAN, // spec unknown
            App, // spec unknown
            Decoupled,
            DecoupledPush // does any bank support this?
        }

        public class TanInfo
        {
            public string Name { get; internal set; } = null!;

            public int Format { get; internal set; }

            public int MaxLength { get; internal set; }
        }

        public class DecoupledInfo
        {
            public int MaximumStatusRequests { get; internal set; }

            public int WaitTimeFirstRequest { get; internal set; }

            public int WaitTimeFollowingRequest { get; internal set; }

            public bool ManualConfirmationAllowed { get; internal set; }

            public bool AutomaticStatusRequestAllowed { get; internal set; }
        }
    }
}
