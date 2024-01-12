using System.Collections;

namespace OnMyRoute.Windows;

public class StateNames(Enum[] elementStates) : IEnumerable<Enum> {
    public IEnumerator<Enum> GetEnumerator() => ((IEnumerable<Enum>)elementStates).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => elementStates.GetEnumerator();
}
