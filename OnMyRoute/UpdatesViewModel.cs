using Microsoft.Extensions.Options;
using OnMyRoute.Input;
using System.ComponentModel;
using System.Net.Http;
using Updates.Types;
using Updates.Updates;

namespace OnMyRoute;

public sealed class UpdatesViewModel : INotifyPropertyChanged, IGetCommandProvider, IAsyncDisposable {
    private readonly IUpdateServer updateServer;

    private readonly CommandProvider commandProvider;

    private bool updateOnClose = true;

    private bool preRelease;

    private Update? update;

    private readonly TimeSpan installUpdateWithin;

    public UpdatesViewModel() : this(
        new DesignTimeUpdateServer(),
        Options.Create(
            new UpdateData() {
                Owner = null!,
                RepositoryName = null!,
                Company = null!,
                Product = null!,
                CurrentVersion = null!,
                InstallUpdateWithin = TimeSpan.FromDays(1)
            }
        )
    ) { }

    public UpdatesViewModel(IUpdateServer updateServer, IOptions<UpdateData> updateData) {
        this.updateServer = updateServer;
        installUpdateWithin = updateData.Value.InstallUpdateWithin;
        CurrentVersion = updateData.Value.CurrentVersion;
        commandProvider = new CommandProvider()
            .Register<bool>(UpdateCommands.PreRelease, TogglePreReleaseAsync)
            .Register(UpdateCommands.UpdateNow, UpdateNowAsync);
        commandProvider.Execute(UpdateCommands.PreRelease, false);
    }

    public UpdateState UpdateState { get; private set; }

    public string CurrentVersion { get; }

    public Release? NewVersion { get; private set; }

    public bool UpdateCanBePostponed { get; private set; }

    public DateTime UpdateBefore { get; private set; }

    public string? ErrorMessage { get; private set; }

    public bool UpdateOnClose {
        get => updateOnClose;
        set {
            updateOnClose = value;
            OnPropertyChanged(nameof(UpdateOnClose));
        }
    }

    public bool PreRelease {
        get => preRelease;
        set {
            preRelease = value;
            OnPropertyChanged(nameof(PreRelease));
        }
    }

    async Task TogglePreReleaseAsync(bool preRelease) {
        try {
            PreRelease = preRelease;
            UpdateState = UpdateState.Checking; OnPropertyChanged(nameof(UpdateState));
            NewVersion = await updateServer.CheckForUpdateAsync(preRelease);
            if (NewVersion == null) {
                OnPropertyChanged(nameof(NewVersion));
                UpdateState = UpdateState.Up2date; OnPropertyChanged(nameof(UpdateState));
            } else {
                PreRelease = NewVersion.Prerelease;
                UpdateState = UpdateState.Downloading;
                OnPropertyChanged(nameof(NewVersion));
                OnPropertyChanged(nameof(UpdateState));
                update = await updateServer.DownloadAsync(NewVersion);
                UpdateState = UpdateState.UpdateAvailable;
                updateOnClose = true;
                DateTimeOffset updateBefore = NewVersion.PublishedAt + installUpdateWithin;
                UpdateCanBePostponed = DateTimeOffset.UtcNow < updateBefore;
                UpdateBefore = updateBefore.LocalDateTime;
                OnPropertyChanged(nameof(UpdateState));
                OnPropertyChanged(nameof(UpdateOnClose));
                OnPropertyChanged(nameof(UpdateCanBePostponed));
                OnPropertyChanged(nameof(UpdateBefore));
            }
        } catch (Exception ex) when (ex is Octokit.ForbiddenException || ex is HttpRequestException) {
            UpdateState = UpdateState.Error;
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(UpdateState));
            OnPropertyChanged(nameof(ErrorMessage));
        }
    }

    private Task UpdateNowAsync() {
        if (update != null) {
            updateOnClose = true;
            OnPropertyChanged(nameof(UpdateOnClose));
            Application.Current.Shutdown();
        }
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() {
        if (update != null && UpdateOnClose) {
            updateServer.StartInstallation(update);
        }
        return ValueTask.CompletedTask;
    }

    ICommandProvider IGetCommandProvider.CommandProvider => commandProvider;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
