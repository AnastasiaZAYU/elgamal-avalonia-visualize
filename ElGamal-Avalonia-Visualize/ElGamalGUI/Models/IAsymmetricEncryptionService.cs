using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace ElGamalGUI.Models
{
    public interface IAsymmetricEncryptionService
    {
        ElGamalCiphertext Encrypt(string message, ElGamalParameters parameters, BigInteger publicKey);
        string Decrypt(ElGamalCiphertext ciphertext, ElGamalParameters parameters, BigInteger privateKey);
    }
}
