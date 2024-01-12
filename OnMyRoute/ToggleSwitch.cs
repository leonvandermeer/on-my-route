using System.Windows.Controls.Primitives;

namespace OnMyRoute;

public class ToggleSwitch : ToggleButton {
    static ToggleSwitch() {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleSwitch), new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
    }

    public object CheckedContent {
        get => GetValue(CheckedContentProperty);
        set => SetValue(CheckedContentProperty, value);
    }

    public static readonly DependencyProperty CheckedContentProperty =
        DependencyProperty.Register("CheckedContent", typeof(object), typeof(ToggleSwitch), new PropertyMetadata(null));
}
