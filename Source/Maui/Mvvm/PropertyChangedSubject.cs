using System.ComponentModel;
using System.Linq.Expressions;

namespace Pot.Maui.Mvvm;

internal sealed class PropertyChangedSubject<TViewModel, TProperty> : PropertySubjectBase<TViewModel, TProperty> where TViewModel : ViewModelBase
{
    public PropertyChangedSubject(TViewModel observable, Expression<Func<TViewModel, TProperty>> propertyExpression)
        : base(observable, propertyExpression)
    {
        RegisterEventHandler(
            () => { observable.PropertyChanged += HandlePropertyChanged; },
            () => { observable.PropertyChanged -= HandlePropertyChanged; });
    }

    private void HandlePropertyChanged(object? sender, PropertyChangedEventArgs eventArgs)
    {
        NotifyCurrentValueIfRequired(eventArgs.PropertyName);
    }
}
