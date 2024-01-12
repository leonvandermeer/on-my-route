namespace OnMyRoute;

public class MainViewModel(UpdatesViewModel updates) {
    public UpdatesViewModel Updates { get; } = updates;
}
