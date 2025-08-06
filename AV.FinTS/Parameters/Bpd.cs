using AV.FinTS.Exceptions;
using AV.FinTS.Helper;
using AV.FinTS.Raw;
using AV.FinTS.Raw.Segments;
using AV.FinTS.Raw.Segments.Auth;
using AV.FinTS.Raw.Segments.ParameterData;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Parameter
{
    public class Bpd
    {
        public int Version { get; private set; }

        public string BankName { get; private set; } = null!;

        public BankIdentifier Bank { get; private set; } = null!;

        public IReadOnlyCollection<ISegment> Segments { get; private set; } = null!;

        public IReadOnlyCollection<AuthMethod> AuthenticationMethods { get; private set; } = null!;

        public IReadOnlyDictionary<string, bool> PinTanInfo { get; private set; } = null!;

        private Bpd() { }

        public static Bpd FromMessage(RawMessage msg)
        {
            var bpa = msg.Get<HIBPA3>();

            var authMethods = new List<AuthMethod>();
            var hitans7 = msg.Get<HITANS7>();
            foreach (var param in hitans7.Procedures)
            {
                authMethods.Add(AuthMethod.CreateFrom(param));
            }

            var hitans6 = msg.Get<HITANS6>();
            foreach (var param in hitans6.Procedures)
            {
                authMethods.Add(AuthMethod.CreateFrom(param));
            }

            var hipins = msg.Get<HIPINS1>();

            return new Bpd
            {
                Version = bpa.BpdVersion,
                BankName = bpa.BankName,
                Bank = bpa.Bank,
                Segments = msg.Segments.Where(seg => seg.Head.IsBpd).ToList(),
                AuthenticationMethods = authMethods,
                PinTanInfo = hipins.PinTanInfo
            };
        }

        public ParameterSegment GetBestParameterRequired(params Type[] types)
        {
            var ret = GetBestParameter(types);

            if (ret == null)
            {
                throw new OperationNotSupported();
            }
            return ret;
        }

        public ParameterSegment? GetBestParameter(params Type[] types)
        {
            if(types.Length == 0)
            {
                throw new ArgumentException("No type provided");
            }

            if (types.Any(t => !t.IsSubclassOf(typeof(ParameterSegment))))
            {
                throw new ArgumentException("Provided types must extend ParameterSegment");
            }
            
            foreach(var t in types)
            {
                var seg = (ParameterSegment)t.GetConstructor(Type.EmptyTypes)!.Invoke([]);

                var param = (ParameterSegment?)Segments.Where(s => s.Head == seg.Head).FirstOrDefault();

                if(param != null)
                {
                    return param;
                }
            }

            return null;
        }
    }
}
