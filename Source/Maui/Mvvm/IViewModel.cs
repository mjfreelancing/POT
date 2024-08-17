namespace Pot.Maui.Mvvm;

public interface IViewModel
{
    string Title { get; }
    bool IsBusy { get; }
    bool IsNotBusy { get; }

    IDisposable StartIsBusyScope();
}
