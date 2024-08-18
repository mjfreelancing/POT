using AllOverIt.Assertion;
using AllOverIt.Extensions;
using AllOverIt.Patterns.ResourceInitialization;
using System.Linq.Expressions;
using System.Reactive.Subjects;

namespace Pot.Maui.Mvvm;

internal abstract class PropertySubjectBase<TViewModel, TProperty> : IObservable<TProperty>, IDisposable where TViewModel : ViewModelBase
{
    private bool _disposed;
    private Raii? _eventHandlerSubscription;
    private readonly Subject<TProperty> _subject = new();
    private readonly TViewModel _observable;
    private readonly Func<TViewModel, TProperty> _valueResolver;
    private readonly string _propertyName;

    public PropertySubjectBase(TViewModel observable, Expression<Func<TViewModel, TProperty>> propertyExpression)
    {
        _observable = observable;
        _valueResolver = propertyExpression.Compile();
        _propertyName = propertyExpression.UnwrapMemberExpression().Member.Name;
    }

    public IDisposable Subscribe(IObserver<TProperty> observer)
    {
        Throw<InvalidOperationException>.WhenNull(_eventHandlerSubscription, "The event handler to observe has not been registered.");

        return _subject.Subscribe(observer);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected void RegisterEventHandler(Action eventSubscribe, Action eventUnsubscribe)
    {
        _eventHandlerSubscription = new Raii(eventSubscribe, eventUnsubscribe);
    }

    protected void NotifyCurrentValueIfRequired(string? propertyName)
    {
        if (_propertyName == propertyName)
        {
            var value = _valueResolver.Invoke(_observable);

            _subject.OnNext(value);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _eventHandlerSubscription?.Dispose();
                _subject.Dispose();
            }

            _disposed = true;
        }
    }
}
