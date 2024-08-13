namespace Pot.Maui.Features;

public interface IViewModel
{
    string Title { get; }
    bool IsBusy { get; }
    bool IsNotBusy { get; }

    IDisposable GetIsBusyTransaction();
}
