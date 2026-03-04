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

namespace ElGamalGUI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly ElGamalService _elGamalService;
    private ElGamalParameters? _parameters;
    private ElGamalKeyPair? _keyPair;

    public ElGamalKeyPair? KeyPair { get => _keyPair; set => this.RaiseAndSetIfChanged(ref _keyPair, value); }

    private string _inputMessage = "Hello, Avalonia!";
    public string InputMessage { get => _inputMessage; set => this.RaiseAndSetIfChanged(ref _inputMessage, value); }


    private string _statusMessage = "Ready to work";
    public string StatusMessage { get => _statusMessage; set => this.RaiseAndSetIfChanged(ref _statusMessage, value); } //set => SetProperty(ref _statusMessage, value); }


    private string _resultText = string.Empty;
    public string ResultText { get => _resultText; set => this.RaiseAndSetIfChanged(ref _resultText, value); }

    public ReactiveCommand<Unit, Unit> GenerateKeysCommand { get; }
    public ReactiveCommand<Unit, Unit> EncryptCommand { get; }
    public ReactiveCommand<Unit, Unit> DecryptCommand { get; }
    public ReactiveCommand<Unit, Unit> SignCommand { get; }
    public ReactiveCommand<Unit, Unit> VerifyCommand { get; }

    public MainWindowViewModel()
    {
        _elGamalService = new ElGamalService();
        var canEncrypt = this.WhenAnyValue(
            x => x.KeyPair,
            (ElGamalKeyPair? k) => k != null
            );
        GenerateKeysCommand = ReactiveCommand.CreateFromTask(GenerateKeysAsync);
        EncryptCommand = ReactiveCommand.Create(EncryptMessage, canEncrypt);
        DecryptCommand = ReactiveCommand.Create(DecryptMessage, canEncrypt);
        SignCommand = ReactiveCommand.Create(SignMessage, canEncrypt);
        VerifyCommand = ReactiveCommand.Create(VerifySignature, canEncrypt);
    }

    private async Task GenerateKeysAsync()
    {
        StatusMessage = "Generating system... Please wait.";

        try
        {
            var results = await Task.Run(() =>
            {
                var p = _elGamalService.GenerateParameters(1024);
                var k = _elGamalService.GenerateKeyPair(p);
                return (Parameters: p, KeyPair: k);
            });

            _parameters = results.Parameters;
            KeyPair = results.KeyPair;
            StatusMessage = "Keys generated successfully!";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private void EncryptMessage()
    {
        if (_keyPair is ElGamalKeyPair keys && _parameters is ElGamalParameters paramsCurrent)
        {
            var cipher = _elGamalService.Encrypt(InputMessage, paramsCurrent, keys.PublicKey);
            string blockY = string.Join(":", cipher.Y.Select(b => b.ToString("X")));
            ResultText = $"{cipher.X.ToString("X")}|{blockY}";
            StatusMessage = "Message encrypted.";
        }
        else { StatusMessage = "First, generate the keys"; }
    }

    private void DecryptMessage()
    {
        if (_keyPair is ElGamalKeyPair keys && _parameters is ElGamalParameters paramsCurrent)
        {
            try
            {
                var parts = ResultText.Split('|');
                if (parts.Length < 2)
                    throw new Exception("Invalid format");

                BigInteger x = BigInteger.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
                var yBlocks = parts[1].Split(':')
                    .Select(hex => BigInteger.Parse(hex, System.Globalization.NumberStyles.HexNumber))
                    .ToList();
                var ciphertext = new ElGamalCiphertext(x, yBlocks);
                string decrypted = _elGamalService.Decrypt(ciphertext, paramsCurrent, keys.PrivateKey);
                ResultText = decrypted;
                StatusMessage = "Message decrypted.";
            }
            catch (Exception ex)
            {
                StatusMessage = "Decryption failed. Check keys or input.";
            }
        }
    }

    private void SignMessage()
    {
        if (_keyPair is ElGamalKeyPair keys && _parameters is ElGamalParameters paramsCurrent)
        {
            try
            {
                var signature = _elGamalService.Sign(InputMessage, paramsCurrent, keys.PrivateKey);
                ResultText = $"{signature.R.ToString("X")}|{signature.S.ToString("X")}";
                StatusMessage = "Message signed.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Signing failed: {ex.Message}";
            }
        }
    }

    private void VerifySignature()
    {
        if (_keyPair is ElGamalKeyPair keys && _parameters is ElGamalParameters paramsCurrent)
        {
            try
            {
                var parts = ResultText.Split('|');
                if (parts.Length < 2)
                    throw new Exception("Invalid signature format. Use R|S");

                BigInteger r = BigInteger.Parse(parts[0], System.Globalization.NumberStyles.HexNumber);
                BigInteger s = BigInteger.Parse(parts[1], System.Globalization.NumberStyles.HexNumber);

                var signature = new ElGamalSignature(r, s);
                bool isValid = _elGamalService.Verify(InputMessage, signature, paramsCurrent, keys.PublicKey);

                if (isValid)
                {
                    StatusMessage = "✅ Signature is VALID!";
                    ResultText = "SUCCESS: The message is authentic.";
                }
                else
                {
                    StatusMessage = "❌ Signature is INVALID!";
                    ResultText = "WARNING: The message may have been tampered with.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Verification error. Check signature format.";
            }
        }
    }
}


    /*private string _pText = "Not generated";
    private string _gText = "Not generated";

    private string _privateKeyText = "Not generated";
    private string _publicKeyText = "Not generated";
    private string _statusText = "Ready";

    #region Properties
    public string PText { get => _pText; set => SetProperty(ref _pText, value); }
    public string GText { get => _gText; set => SetProperty(ref _gText, value); }
    public string PrivateKeyText { get => _privateKeyText; set => SetProperty(ref _privateKeyText, value); }
    public string PublicKeyText { get => _publicKeyText; set => SetProperty(ref _publicKeyText, value); }
    public string StatusText { get => _statusText; set => SetProperty(ref _statusText, value); }
    #endregion

    public ReactiveCommand<Unit, Unit> GenerateAllCommand { get; }

    public MainWindowViewModel()
    {
        GenerateAllCommand = ReactiveCommand.CreateFromTask(GenerateFullSystemAsync);
    }

    private async Task GenerateFullSystemAsync()
    {
        // marshal the UI update to the UI thread
        Dispatcher.UIThread.Post(() => StatusText = "Generating 3072-bit system... Please wait.");

        try
        {
            var result = await Task.Run(() => { /* heavy work *//* });

            Dispatcher.UIThread.Post(() =>
            {
                PText = result.parameters.P.ToString("X");
                GText = result.parameters.G.ToString("X");
                PrivateKeyText = result.keyPair.PrivateKey.ToString("X");
                PublicKeyText = result.keyPair.PublicKey.ToString("X");
                StatusText = "System generated successfully!";
            });
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(() => StatusText = $"Error: {ex.Message}");
        }
    }
}*/

