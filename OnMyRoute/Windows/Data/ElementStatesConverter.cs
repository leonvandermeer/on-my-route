using System.Globalization;
using System.Windows.Data;

namespace OnMyRoute.Windows.Data;

public class ElementStatesConverter : IMultiValueConverter {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
        List<Enum> elementStates = new(values.Length);
        for (int i = 0; i < values.Length; i++) {
            if (values[i] != DependencyProperty.UnsetValue) {
                elementStates.Add((Enum)values[i]);
            }
        }
        return new StateNames([.. elementStates]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
