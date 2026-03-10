using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElGamalGUI.Models;

public readonly struct ElGamalKeyPair : IFormattable
{
    public BigInteger PrivateKey { get; init; }
    public BigInteger PublicKey { get; init; }

    public ElGamalKeyPair(BigInteger privateKey, BigInteger publicKey)
    {
        PrivateKey = privateKey;
        PublicKey = publicKey;
    }

    public bool IsEmpty => PrivateKey == 0 && PublicKey == 0;

    public override string ToString() => ToString(null, null);

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        return format?.ToUpper() switch
        {
            "PRIVATEKEY" or "PRIV" => $"0x{PrivateKey.ToString("X")}",
            "PUBLICKEY" or "PUB" => $"0x{PublicKey.ToString("X")}",
            _ => $"Private Key: 0x{PrivateKey.ToString("X")}\nPublic Key: 0x{PublicKey.ToString("X")}\n",
        };
    }
}
