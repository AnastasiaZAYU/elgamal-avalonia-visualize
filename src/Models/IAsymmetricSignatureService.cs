using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElGamalGUI.Models;

public interface IAsymmetricSignatureService
{
    ElGamalSignature Sign(string message, ElGamalParameters parameters, BigInteger privateKey);
    bool Verify(string message, ElGamalSignature signature, ElGamalParameters parameters, BigInteger publicKey);
}
