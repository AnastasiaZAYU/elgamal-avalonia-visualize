using ElGamalGUI.Models;
using System.Numerics;

namespace ElGamalGUI.Tests.Models;

public class ElGamalServiceTests
{
    private readonly ElGamalService _service;

    public ElGamalServiceTests()
    {
        _service = new ElGamalService();
    }
    private (ElGamalParameters parameters, ElGamalKeyPair keys) GetTestSetup()
    {
        var parameters = _service.GenerateParameters(512);
        var keys = _service.GenerateKeyPair(parameters);
        return (parameters, keys);
    }

    [Fact]
    public void Signature_ShouldBeValid_ForOriginalMessage()
    {
        // Arrange
        var (parameters, keyPair) = GetTestSetup();
        string message = "Standard test message";

        // Act
        var signature = _service.Sign(message, parameters, keyPair.PrivateKey);
        bool isValid = _service.Verify(message, signature, parameters, keyPair.PublicKey);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Signature_ShouldBeInvalid_ForModifiedMessage()
    {
        // Arrange
        var (parameters, keyPair) = GetTestSetup();
        string message = "Original message";
        var signature = _service.Sign(message, parameters, keyPair.PrivateKey);

        // Act
        bool isValid = _service.Verify("Modified message", signature, parameters, keyPair.PublicKey);

        // Assert
        Assert.False(isValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Short")]
    [InlineData("Very long message that will definitely be split into multiple blocks because its length exceeds the key size significantly...")]
    [InlineData("   Message with spaces   ")]
    [InlineData("!2#$%^&*()_+ symbols")]
    [InlineData("Привіт, світ!")]
    [InlineData("🚀 Crypto 🔐")]
    public void Encryption_ShouldRestoreOriginalText_ForVariousInputs(string originalMessage)
    {
        // Act
        var (parameters, keyPair) = GetTestSetup();
        var ciphertext = _service.Encrypt(originalMessage, parameters, keyPair.PublicKey);
        string decryptedMessage = _service.Decrypt(ciphertext, parameters, keyPair.PrivateKey);

        // Assert
        Assert.Equal(originalMessage, decryptedMessage);
    }

    [Fact]
    public void Decryption_WithWrongKey_ShouldFail()
    {
        // Arrange
        var (parameters, keyPair) = GetTestSetup();
        string message = "Secret data";
        var secondKeyPair = _service.GenerateKeyPair(parameters);
        var ciphertext = _service.Encrypt(message, parameters, keyPair.PublicKey);

        // Act
        string decryptedWithWrongKey = _service.Decrypt(ciphertext, parameters, secondKeyPair.PrivateKey);

        // Assert
        Assert.NotEqual(message, decryptedWithWrongKey);
    }

    [Fact]
    public void Math_ModInverse_ShouldThrow_WhenNoInverseExists()
    {
        // Arrange 
        BigInteger a = 10;
        BigInteger n = 20;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ElGamalMath.ModInverse(a, n));
    }

    [Fact]
    public void Encrypt_SameMessageTwice_ShouldProduceDifferentCiphertexts()
    {
        // Arrange
        var (parameters, keyPair) = GetTestSetup();
        string message = "Test";

        // Act
        var cipher1 = _service.Encrypt(message, parameters, keyPair.PublicKey);
        var cipher2 = _service.Encrypt(message, parameters, keyPair.PublicKey);

        // Assert
        Assert.NotEqual(cipher1, cipher2);
    }
}