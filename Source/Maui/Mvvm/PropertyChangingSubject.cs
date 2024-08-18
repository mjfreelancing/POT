using System.Linq.Expressions;

namespace Pot.Maui.Mvvm;

internal sealed class PropertyChangingSubject<TViewModel, TProperty> : PropertySubjectBase<TViewModel, TProperty> where TViewModel : ViewModelBase
{
    public PropertyChangingSubject(TViewModel observable, Expression<Func<TViewModel, TProperty>> propertyExpression)
        : base(observable, propertyExpression)
    {
        RegisterEventHandler(
            () => { observable.PropertyChanging += HandlePropertyChanging; },
            () => { observable.PropertyChanging -= HandlePropertyChanging; });
    }

    private void HandlePropertyChanging(object? sender, System.ComponentModel.PropertyChangingEventArgs eventArgs)
    {
        NotifyCurrentValueIfRequired(eventArgs.PropertyName);
    }
}