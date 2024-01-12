using Microsoft.Build.Framework;
using Task = Microsoft.Build.Utilities.Task;

namespace InnoSetup.Tasks;

public class CreateInnoSetupFiles : Task {

    [Required]
    public ITaskItem[]? PublishItems { get; set; }

    [Required]
    public ITaskItem? OutputFile { get; set; }

    public override bool Execute() {
        try {
            ExecuteImpl();
        } catch (Exception ex) {
            Log.LogErrorFromException(ex, true, true, null);
        }
        return !Log.HasLoggedErrors;
    }

    private void ExecuteImpl() {
        string scriptPath = OutputFile!.ItemSpec;
        using TextWriter script = File.CreateText(scriptPath);
        FilesSection(script);
    }

    private void FilesSection(TextWriter script) {
        script.WriteLine("[Files]");
        foreach (ITaskItem item in PublishItems!) {
            string source = Helpers.RelativePathTo(OutputFile!.ItemSpec, item.GetMetadata("OutputPath"));
            string relDir = Path.GetDirectoryName(item.GetMetadata("RelativePath"));
            string destDir = Path.Combine("{app}", relDir);
            string destFile = Path.Combine(destDir, Path.GetFileName(source));
            script.WriteLine(
                "Source: \"{0}\"; DestDir: \"{1}\"; Flags: ignoreversion; AfterInstall: OnInstalled('{2}')",
                source, destDir, destFile
            );
        }
    }
}
