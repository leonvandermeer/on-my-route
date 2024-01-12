using System.Windows.Input;

namespace OnMyRoute.Input;

class CommandProvider : ICommandProvider {
    private readonly Dictionary<ICommand, AsyncCommand> commands = [];

    public CommandProvider Register(ICommand command, Func<Task> execute) {
        commands.Add(command, new AsyncCommand(s => s == null ? execute() : throw new InvalidOperationException()));
        return this;
    }

    public CommandProvider Register<T>(ICommand command, Func<T, Task> execute) {
        commands.Add(command, new AsyncCommand(s => execute((T)s)));
        return this;
    }

    public bool CanExecute(ICommand command) =>
        commands[command].CanExecute();

    public void Execute(ICommand command, object parameter) =>
        commands[command].Execute(parameter);
}
