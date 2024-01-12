using System.Windows.Input;

namespace OnMyRoute.Input;

class CommandProviderBinding : CommandBinding {
    public CommandProviderBinding() {
        CanExecute += OnCanExecute;
        Executed += OnExecuted;
    }

    private void OnCanExecute(object sender, CanExecuteRoutedEventArgs e) {
        ICommandProvider? commandProvider = GetCommandProvider(sender);
        if (commandProvider != null) {
            e.CanExecute = commandProvider.CanExecute(e.Command);
            e.Handled = true;
        }
    }

    private void OnExecuted(object sender, ExecutedRoutedEventArgs e) {
        GetCommandProvider(sender)!.Execute(e.Command, e.Parameter);
        e.Handled = true;
    }

    private static ICommandProvider? GetCommandProvider(object sender) {
        FrameworkElement view = (FrameworkElement)sender;
        IGetCommandProvider getCommandProvider = (IGetCommandProvider)view.DataContext;
        return getCommandProvider?.CommandProvider;
    }
}
