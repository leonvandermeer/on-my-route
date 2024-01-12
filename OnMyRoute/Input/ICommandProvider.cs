using System.Windows.Input;

namespace OnMyRoute.Input;

interface ICommandProvider {
    bool CanExecute(ICommand command);

    void Execute(ICommand command, object parameter);
}
