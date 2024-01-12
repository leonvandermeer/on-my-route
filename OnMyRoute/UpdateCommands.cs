using System.Windows.Input;

namespace OnMyRoute;

class UpdateCommands {
    public static readonly RoutedUICommand PreRelease = new("Pre-Release", nameof(PreRelease), typeof(UpdateCommands));
    public static readonly RoutedUICommand UpdateNow = new("Update Now", nameof(UpdateNow), typeof(UpdateCommands));
}
