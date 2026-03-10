using System;
using System.Threading.Tasks;
using System.Numerics;
using System.Reactive;
using ReactiveUI;
using ElGamalGUI.Models;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Utilities;
using Avalonia.Metadata;
using System.Windows.Input;

namespace ElGamalGUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ElGamalService _elGamalService;
    private ElGamalParameters _parameters;
    private ElGamalKeyPair _keyPair;

    private readonly int _keySize;
    private string _pText = string.Empty;
    private string _gText = string.Empty;
    private string _publicKeyText = string.Empty;
    private string _inputMessage = string.Empty;
    private string _ciphertext = string.Empty;
    private string _signatureText = string.Empty;
    private string _isValidText = string.Empty;
    private bool _isBusy = false;
    private bool _areKeysGenerated = false;
    private string _statusText = "Generate keys first!";

    public MainWindowViewModel(int keySize = 3072)
    {
        _keySize = keySize;

        _elGamalService = new ElGamalService();
        GenerateKeysCommand = ReactiveCommand.CreateFromTask(GenerateKeysAsync);
        EncryptCommand = ReactiveCommand.Create(EncryptMessage);
        DecryptCommand = ReactiveCommand.Create(DecryptMessage);
        SignCommand = ReactiveCommand.Create(SignMessage);
        VerifyCommand = ReactiveCommand.Create(VerifySignature);
    }

    public string PText { get => _pText; set => this.RaiseAndSetIfChanged(ref _pText, value); }
    public string GText { get => _gText; set => this.RaiseAndSetIfChanged(ref _gText, value); }
    public string PublicKeyText { get => _publicKeyText; set => this.RaiseAndSetIfChanged(ref _publicKeyText, value); }
    public string InputMessage { get => _inputMessage; set => this.RaiseAndSetIfChanged(ref _inputMessage, value); }
    public string CiphertextText { get => _ciphertext; set => this.RaiseAndSetIfChanged(ref _ciphertext, value); }
    public string SignatureText { get => _signatureText; set => this.RaiseAndSetIfChanged(ref _signatureText, value); }
    public string IsValidText { get => _isValidText; set => this.RaiseAndSetIfChanged(ref _isValidText, value); }
    public bool IsBusy { get => _isBusy; set => this.RaiseAndSetIfChanged(ref _isBusy, value); }
    public bool AreKeysGenerated { get => _areKeysGenerated; set => this.RaiseAndSetIfChanged(ref _areKeysGenerated, value); }
    public string StatusText { get => _statusText; set => this.RaiseAndSetIfChanged(ref _statusText, value); }


    public ReactiveCommand<Unit, Unit> GenerateKeysCommand { get; }
    public ReactiveCommand<Unit, Unit> EncryptCommand { get; }
    public ReactiveCommand<Unit, Unit> DecryptCommand { get; }
    public ReactiveCommand<Unit, Unit> SignCommand { get; }
    public ReactiveCommand<Unit, Unit> VerifyCommand { get; }

    private async Task GenerateKeysAsync()
    {
        IsBusy = true;
        StatusText = "Generating system... Please wait.";

        try
        {
            await Task.Run(() =>
            {
                var paramsResult = _elGamalService.GenerateParameters(_keySize);
                var keysResult = _elGamalService.GenerateKeyPair(paramsResult);

                _parameters = paramsResult;
                _keyPair = keysResult;

                PText = $"{_parameters:P}";
                GText = $"{_parameters:G}";
                PublicKeyText = $"{_keyPair:PublicKey}";
            });

            CiphertextText = string.Empty;
            SignatureText = string.Empty;
            IsValidText = string.Empty;

            AreKeysGenerated = true;
            StatusText = "New keypair generated. Ready to work.";
        }
        catch (Exception ex)
        {
            StatusText = $"Error during generation: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void EncryptMessage()
    {
        StatusText = "Processing...";

        if (string.IsNullOrWhiteSpace(InputMessage))
            return;

        if (_parameters.IsEmpty || _keyPair.IsEmpty)
        {
            StatusText = "Error: Generate keys first!";
            return;
        }

        try
        {
            var ciphertext = _elGamalService.Encrypt(InputMessage, _parameters, _keyPair.PublicKey);
            CiphertextText = ciphertext.ToString();
            StatusText = "Ready to work.";
        }
        catch (Exception ex)
        {
            StatusText = $"Encryption error: {ex.Message}";
            CiphertextText = string.Empty;
            IsValidText = string.Empty;
        }
    }

    private void DecryptMessage()
    {
        StatusText = "Processing..."; 

        if (string.IsNullOrWhiteSpace(CiphertextText))
            return;

        if (_parameters.IsEmpty || _keyPair.IsEmpty)
        {
            StatusText = "Error: Generate keys first!";
            return;
        }

        try
        {
            var parts = CiphertextText.Split('|');
            if (parts.Length != 2)
                throw new Exception("Invalid ciphertext format. Expected 'X|Y'");

            string xStr = parts[0].Replace("0x", "", StringComparison.OrdinalIgnoreCase);
            BigInteger x = BigInteger.Parse(xStr, System.Globalization.NumberStyles.HexNumber);

            var yParts = parts[1].Split(':');
            var yBlocks = yParts
              .Select(p => p.Replace("0x", "", StringComparison.OrdinalIgnoreCase))
              .Select(p => BigInteger.Parse(p, System.Globalization.NumberStyles.HexNumber))
              .ToList();

            var ciphertext = new ElGamalCiphertext { X = x, Y = yBlocks };
            InputMessage = _elGamalService.Decrypt(ciphertext, _parameters, _keyPair.PrivateKey);
            StatusText = "Ready to work.";
        }
        catch (Exception ex)
        {
            StatusText = $"Decryption error: {ex.Message}";
            InputMessage = string.Empty;
        }
        finally
        {
            IsValidText = string.Empty;
        }
    }

    public void SignMessage()
    {
        StatusText = "Processing...";

        if (string.IsNullOrWhiteSpace(InputMessage))
        {
            StatusText = "Error: Message is empty!";
            return;
        }

        if (_parameters.IsEmpty || _keyPair.IsEmpty)
        {
            StatusText = "Error: Generate keys first!";
            return;
        }

        try
        {
            var signature = _elGamalService.Sign(InputMessage, _parameters, _keyPair.PrivateKey);
            SignatureText = signature.ToString();
            StatusText = "Ready to work.";
        }
        catch (Exception ex)
        {
            StatusText = $"Signing error: {ex.Message}";
            SignatureText = string.Empty;
            IsValidText = string.Empty;
        }
        finally
        {
            IsValidText = string.Empty;
        }
    }

    public void VerifySignature()
    {
        StatusText = "Processing...";

        if (string.IsNullOrWhiteSpace(InputMessage) || string.IsNullOrWhiteSpace(SignatureText))
        {
            StatusText = "Missing message or signature!";
            return;
        }

        if (_parameters.IsEmpty || _keyPair.IsEmpty)
        {
            StatusText = "Error: Generate keys first!";
            return;
        }

        try
        {
            var parts = SignatureText.Split('|');
            if (parts.Length != 2)
                throw new Exception("Invalid signature format. Expected 'R|S'");

            string rStr = parts[0].Replace("0x", "", StringComparison.OrdinalIgnoreCase);
            string sStr = parts[1].Replace("0x", "", StringComparison.OrdinalIgnoreCase);

            BigInteger r = BigInteger.Parse(rStr, System.Globalization.NumberStyles.HexNumber);
            BigInteger s = BigInteger.Parse(sStr, System.Globalization.NumberStyles.HexNumber);

            var signature = new ElGamalSignature { R = r, S = s };
            bool isValid = _elGamalService.Verify(InputMessage, signature, _parameters, _keyPair.PublicKey);

            IsValidText = isValid
              ? "✅ Signature is Valid"
              : "❌ Signature is Compromised!";

            StatusText = "Ready to work.";
        }
        catch (Exception ex)
        {
            StatusText = $"Verification error: {ex.Message}";
            IsValidText = string.Empty;
        }
    }
}