using System.Windows.Data;

namespace OnMyRoute.Windows.Data;

public class ElementStateBinding : MultiBinding {
    private static readonly ElementStatesConverter converter = new();

    public ElementStateBinding() => Converter = converter;
}
