using ElGamalGUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;

namespace ElGamalGUI.Tests.ViewModels;

public class MainWindowViewModelTests
{
    private readonly MainWindowViewModel _viewModel;

    public MainWindowViewModelTests()
    {
        _viewModel = new MainWindowViewModel(512);
    }

    [Fact]
    public async Task GenerateKeysCommand_ShouldUpdateParameters()
    {
        // Act
        await _viewModel.GenerateKeysCommand.Execute(Unit.Default);

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.PText));
        Assert.False(string.IsNullOrEmpty(_viewModel.GText));
        Assert.False(string.IsNullOrEmpty(_viewModel.PublicKeyText));
        Assert.Contains("generated", _viewModel.StatusText.ToLower());
    }

    [Fact]
    public async Task GenerateKeysCommand_ShouldUpdateKeysAndClearPreviousResults()
    {
        // Arrange
        _viewModel.CiphertextText = "old-ciphertext-data";
        _viewModel.SignatureText = "old-signature-data";
        _viewModel.IsValidText = "Signature is Valid";

        // Act
        await _viewModel.GenerateKeysCommand.Execute(Unit.Default);

        // Assert
        Assert.False(string.IsNullOrEmpty(_viewModel.PText));
        Assert.False(string.IsNullOrEmpty(_viewModel.GText));
        Assert.False(string.IsNullOrEmpty(_viewModel.PublicKeyText));

        Assert.True(string.IsNullOrEmpty(_viewModel.CiphertextText));
        Assert.True(string.IsNullOrEmpty(_viewModel.SignatureText));
        Assert.True(string.IsNullOrEmpty(_viewModel.IsValidText));

        Assert.Contains("generated", _viewModel.StatusText.ToLower());
    }

    [Fact]
    public async Task VerifyCommand_WithValidSignature_ShouldSetSuccessStatus()
    {
        // Arrange
        await _viewModel.GenerateKeysCommand.Execute(Unit.Default);
        _viewModel.InputMessage = "Test message";
        await _viewModel.SignCommand.Execute(Unit.Default);

        // Act
        await _viewModel.VerifyCommand.Execute(Unit.Default);

        // Assert
        Assert.Contains("Valid", _viewModel.IsValidText);
    }

    [Fact]
    public async Task VerifyCommand_WithTamperedMessage_ShouldSetErrorStatus()
    {
        // Arrange
        await _viewModel.GenerateKeysCommand.Execute(Unit.Default);
        _viewModel.InputMessage = "Original message";
        await _viewModel.SignCommand.Execute(Unit.Default);

        // Act
        _viewModel.InputMessage = "Tampered message";
        await _viewModel.VerifyCommand.Execute(Unit.Default);

        // Assert
        Assert.Contains("Compromised", _viewModel.IsValidText);
    }
}