namespace OnMyRoute.Windows;

public static class VisualStates {
    public static StateNames GetGoToElementState(DependencyObject obj) {
        return (StateNames)obj.GetValue(GoToElementStateProperty);
    }

    public static void SetGoToElementState(DependencyObject obj, StateNames value) {
        obj.SetValue(GoToElementStateProperty, value);
    }

    public static readonly DependencyProperty GoToElementStateProperty =
        DependencyProperty.RegisterAttached("GoToElementState", typeof(StateNames), typeof(VisualStates), new PropertyMetadata(null, OnGoToElementStateChanged));

    private static void OnGoToElementStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        FrameworkElement stateGroupsRoot = (FrameworkElement)d;
        bool useTransitions = e.OldValue != null;
        StateNames stateNames = (StateNames)e.NewValue;
        foreach (Enum stateName in stateNames) {
            _ = VisualStateManager.GoToElementState(stateGroupsRoot, stateName.ToString(), useTransitions);
        }
    }
}
