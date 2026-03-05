using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Linq;

namespace ElGamalGUI.Models
{
    public readonly struct ElGamalCiphertext
    {
        public BigInteger X { get; init; }
        public List<BigInteger> Y { get; init; }

        public ElGamalCiphertext(BigInteger x, List<BigInteger> y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() =>
            $"0x{X:X}|{string.Join(":", Y.Select(b => "0x" + b.ToString("X")))}";
    }
}
