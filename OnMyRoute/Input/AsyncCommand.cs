using System.Windows.Input;

namespace OnMyRoute.Input;

class AsyncCommand(Func<object, Task> execute) {
    private bool running;

    public bool Running {
        get => running;
        set {
            running = value;
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool CanExecute() => !Running;

    public async void Execute(object parameter) {
        Running = true;
        try {
            await execute(parameter);
        } finally {
            Running = false;
        }
    }
}
