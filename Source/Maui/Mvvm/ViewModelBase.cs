using AllOverIt.Extensions;
using AllOverIt.Patterns.ResourceInitialization;
using AllOverIt.Reactive.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace Pot.Maui.Mvvm;

public partial class ViewModelBase : ObservableObject, IViewModel
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;


    // This is disposable
    public ObservableProperty<bool> BusyChanged { get; } = new();


    public IDisposable StartIsBusyScope()
    {
        // TODO: Create a reference counted version so it can handle nested calls
        return new Raii(
            () => { IsBusy = true; },
            () => { IsBusy = false; });
    }

    public virtual void OnActivate(CompositeDisposable disposables)
    {
        BindPropertyChangedToObservable<ViewModelBase, bool>(vm => vm.IsBusy, BusyChanged, disposables);
    }

    public virtual void OnDeactivate()
    {
    }


    private void BindPropertyChangedToObservable<TViewModel, TProperty>(Expression<Func<TViewModel, TProperty>> propertyExpression,
        ObservableProperty<TProperty> observableProperty, CompositeDisposable disposables) where TViewModel : ViewModelBase
    {
        // An observable that will emit a value when the property value is changed.
        var observable = new PropertyToObservable<TViewModel, TProperty>((TViewModel)this, propertyExpression);

        // Subscribe to the observable, and forward the emitted value to the observable property.
        observableProperty.BindTo(observable).DisposeUsing(disposables);

        observable.DisposeUsing(disposables);
    }
}



public sealed class ObservableProperty<TProperty> : IObservable<TProperty>, IDisposable
{
    private bool _disposed;
    private readonly Subject<TProperty> _valueChanged = new();

    public IDisposable Subscribe(IObserver<TProperty> observer)
    {
        return _valueChanged.Subscribe(observer);
    }

    // When the observable emits a value, subscribe for notification
    public IDisposable BindTo(IObservable<TProperty> observable)
    {
        return observable.Subscribe(_valueChanged);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _valueChanged.Dispose();
            _disposed = true;
        }
    }
}



public sealed class PropertyToObservable<TViewModel, TProperty> : IObservable<TProperty>, IDisposable where TViewModel : ViewModelBase
{
    private bool _disposed;
    private readonly Subject<TProperty> _subject = new();
    private readonly TViewModel _observable;
    private readonly Func<TViewModel, TProperty> _valueResolver;
    private readonly string _propertyName;

    public PropertyToObservable(TViewModel observable, Expression<Func<TViewModel, TProperty>> propertyExpression)
    {
        _observable = observable;
        _valueResolver = propertyExpression.Compile();
        _propertyName = propertyExpression.UnwrapMemberExpression().Member.Name;
        _observable.PropertyChanged += HandlePropertyChanged;
    }

    public IDisposable Subscribe(IObserver<TProperty> observer)
    {
        return _subject.Subscribe(observer);
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == _propertyName)
        {
            var value = _valueResolver.Invoke(_observable);

            _subject.OnNext(value);
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _subject.Dispose();
                _observable.PropertyChanged -= HandlePropertyChanged;
            }

            _disposed = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}