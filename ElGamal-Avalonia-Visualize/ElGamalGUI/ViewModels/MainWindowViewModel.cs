using System.Threading.Tasks;
using System.Reactive;
using ReactiveUI;
using ElGamalGUI.Models;

namespace ElGamalGUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private string _pText = "Not generated";
    private string _gText = "Not generated";

    public string PText
    {
        get => _pText;
        set => SetProperty(ref _pText, value);
    }

    public string GText
    {
        get => _gText;
        set => SetProperty(ref _gText, value);
    }

    public ReactiveCommand<Unit, Unit> GenerateKeysCommand { get; }

    public MainWindowViewModel()
    {
        GenerateKeysCommand = ReactiveCommand.CreateFromTask(GenerateKeysAsync);
    }

    private async Task GenerateKeysAsync()
    {
        var service = new ElGamalService();

        await Task.Run(() =>
        {
            var parameters = service.GenerateParameters(3072);
            PText = parameters.P.ToString();
            GText = parameters.G.ToString();
        });
    }
}

