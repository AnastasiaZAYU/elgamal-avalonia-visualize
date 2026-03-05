using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElGamalGUI.Models
{
    public readonly struct ElGamalParameters : IFormattable
    {
        public BigInteger P { get; init; }
        public BigInteger G { get; init; }

        public ElGamalParameters(BigInteger p, BigInteger g)
        {
            P = p;
            G = g;
        }

        public bool IsEmpty => P == 0 && G == 0;

        public override string ToString() => ToString(null, null);

        public string ToString(string? format, IFormatProvider? formatProvider = null)
        {
            return format?.ToUpper() switch
            {
                "P" => $"0x{P.ToString("X")}",
                "G" => $"0x{G.ToString("X")}",
                _ => $"P: 0x{P.ToString("X")}\nG: 0x{G.ToString("X")}\n",
            };
        }
    }
}
