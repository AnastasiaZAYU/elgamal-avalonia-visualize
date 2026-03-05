using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ElGamalGUI.Models
{
    public readonly struct ElGamalSignature
    {
        public BigInteger R { get; init; }
        public BigInteger S { get; init; }

        public ElGamalSignature(BigInteger r, BigInteger s)
        {
            R = r;
            S = s;
        }

        public override string ToString() => $"0x{R:X}|0x{S:X}";
    }
}
