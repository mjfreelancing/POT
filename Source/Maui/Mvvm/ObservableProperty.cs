using AllOverIt.Reactive.Extensions;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Pot.Maui.Mvvm;

public sealed class ObservableProperty<TProperty> : IObservable<TProperty>, IDisposable
{
    private bool _disposed;
    private readonly Subject<TProperty> _valueChanged = new();

    public void BindToPropertyChanging<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, TProperty>> propertyExpression,
        CompositeDisposable disposables) where TViewModel : ViewModelBase
    {
        // An observable that will emit a value when the property value is changed.
        var observable = new PropertyChangingSubject<TViewModel, TProperty>(viewModel, propertyExpression);

        SetupSubscriptionAndLifetime(observable, disposables);
    }

    public void BindToPropertyChanged<TViewModel>(TViewModel viewModel, Expression<Func<TViewModel, TProperty>> propertyExpression,
        CompositeDisposable disposables) where TViewModel : ViewModelBase
    {
        // An observable that will emit a value when the property value is changed.
        var observable = new PropertyChangedSubject<TViewModel, TProperty>(viewModel, propertyExpression);

        SetupSubscriptionAndLifetime(observable, disposables);
    }

    public IDisposable Subscribe(IObserver<TProperty> observer)
    {
        return _valueChanged.Subscribe(observer);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _valueChanged.Dispose();
            _disposed = true;
        }
    }

    private void SetupSubscriptionAndLifetime<TViewModel>(PropertySubjectBase<TViewModel, TProperty> observable, CompositeDisposable disposables)
        where TViewModel : ViewModelBase
    {
        // Subscribe to the observable, and forward the emitted value to the observable property.
        observable
            .Subscribe(_valueChanged)
            .DisposeUsing(disposables);

        observable.DisposeUsing(disposables);
    }
}
