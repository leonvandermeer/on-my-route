using Microsoft.Build.Framework;
using System.Security.Cryptography.X509Certificates;
using Task = Microsoft.Build.Utilities.Task;

namespace InnoSetup.Tasks;

public class CreateInnoSetup : Task {

    [Required] public ITaskItem? OutputFile { get; set; }

    [Required] public ITaskItem? AppName { get; set; }

    [Required] public ITaskItem? AppVersion { get; set; }

    [Required] public ITaskItem? AppPublisher { get; set; }

    [Required] public ITaskItem? AppURL { get; set; }

    [Required] public ITaskItem? AppExeName { get; set; }

    [Required] public ITaskItem? VersionInfoTextVersion { get; set; }

    [Required] public ITaskItem? VersionInfoVersion { get; set; }

    public ITaskItem[]? Includes { get; set; }

    [Required] public ITaskItem? OutputBaseFilename { get; set; }

    [Required] public ITaskItem? OutputDir { get; set; }

    [Required] public ITaskItem? CertificateThumbprint { get; set; }

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
        DefineDirectives(script);
        IncludeDirectives(script);
        SetupSection(script);
        CodeSection(script);
    }

    private void DefineDirectives(TextWriter script) {
        script.WriteLine($"#define MyAppName \"{AppName!.ItemSpec}\"");
        script.WriteLine($"#define MyAppVersion \"{AppVersion!.ItemSpec}\"");
        script.WriteLine($"#define MyAppPublisher \"{AppPublisher!.ItemSpec}\"");
        script.WriteLine($"#define MyAppURL \"{AppURL!.ItemSpec}\"");
        script.WriteLine($"#define MyAppExeName \"{AppExeName!.ItemSpec}\"");
        script.WriteLine($"#define MyVersionInfoTextVersion \"{VersionInfoTextVersion!.ItemSpec}\"");
        script.WriteLine($"#define MyVersionInfoVersion \"{VersionInfoVersion!.ItemSpec}\"");
        script.WriteLine();
    }

    private void IncludeDirectives(TextWriter script) {
        if (Includes != null) {
            foreach (ITaskItem item in Includes) {
                string i = Helpers.RelativePathTo(OutputFile!.ItemSpec, item.ItemSpec);
                script.WriteLine($"#include \"{i}\"");
            }
            script.WriteLine();
        }
    }

    private void SetupSection(TextWriter script) {
        script.WriteLine("[Setup]");
        script.WriteLine("AppVersion={#MyAppVersion}");
        script.WriteLine("VersionInfoVersion={#MyVersionInfoVersion}");
        script.WriteLine("VersionInfoTextVersion={#MyVersionInfoTextVersion}");
        script.WriteLine($"OutputBaseFilename={OutputBaseFilename}");
        string o = Helpers.RelativePathTo(OutputFile!.ItemSpec, OutputDir!.ItemSpec);
        script.WriteLine($"OutputDir={o}");
        SignTool(script);
        script.WriteLine();
    }

    private void SignTool(TextWriter script) {
        string thumbprint = CertificateThumbprint!.ItemSpec;
        using X509Store store = new(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);
        using X509Certificate2 cert =
            store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)
            .Cast<X509Certificate2>().FirstOrDefault();
        if (cert != null) {
            script.WriteLine("SignTool=innosetup.tasks");
        } else {
            string message = $"Skip Code Signing because certificate {thumbprint} is not found.";
            script.WriteLine($";SignTool=innosetup.tasks - {message}");
            Log.LogWarning(message);
        }
    }

    private void CodeSection(TextWriter script) {
        script.WriteLine(@"[UninstallDelete]
Type: files; Name: ""{app}\FileListAbsolute.txt""

[Code]
var
  FileWrites: TStringList;
  Superfluous: TStringList;

procedure OnInstalled(FileName: String);
var
  Pos: Integer;
begin
  Log('OnInstalled: ' + FileName);
  FileWrites.Add(ExpandConstant(FileName));
  Pos := Superfluous.IndexOf(ExpandConstant(FileName));
  if Pos >= 0 then
    begin
      Superfluous.Delete(Pos);
    end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
var i: Integer;
begin
  Log('CurStepChanged(' + IntToStr(Ord(CurStep)) + ') called');
  if CurStep = ssInstall then
    begin
      FileWrites := TStringList.Create;
      Superfluous := TStringList.Create;
      if FileExists(ExpandConstant('{app}\FileListAbsolute.txt')) then
        begin
          Superfluous.LoadFromFile(ExpandConstant('{app}\FileListAbsolute.txt'));
        end;
    end
  else if CurStep = ssPostInstall then
    begin
      for i := 0 to Superfluous.Count - 1 do
        begin
          DeleteFile(Superfluous[i]);
          RemoveDir(ExtractFileDir(Superfluous[i]));
        end;
      FileWrites.SaveToFile(ExpandConstant('{app}\FileListAbsolute.txt'));
    end;
end;");
    }
}
