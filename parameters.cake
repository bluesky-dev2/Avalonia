
public class Parameters
{
    public string Configuration { get; private set; }
    public string Artifacts { get; private set; }
    public string VersionSuffix { get; private set; }
    public string Version { get; private set; } = "0.7.0";
    public string NuGetPushBranch { get; private set; }
    public string NuGetPushRepoName { get; private set; }
    public bool PushNuGet { get; private set; }
    public bool IsNugetRelease { get; private set; }
    public (string path, string name)[] BuildProjects { get; private set; }
    public (string path, string name)[] TestProjects { get; private set; }
    public (string path, string name, string framework, string runtime)[] PublishProjects { get; private set; }
    public (string path, string name)[] PackProjects { get; private set; }

    public Parameters(ICakeContext context)
    {
        Configuration = context.Argument("configuration", "Release");
        Artifacts = context.Argument("artifacts", "./artifacts");

        VersionSuffix = context.Argument("suffix", default(string));
        if (VersionSuffix == null)
        {
            var build = context.EnvironmentVariable("BUILD_BUILDNUMBER");
            VersionSuffix = build != null ? $"-build{build}" : "";
        }

        Version += "-build" + int.Parse(context.EnvironmentVariable("BUILD_BUILDNUMBER").Replace(".","").Substring(4)).ToString("D5");

        NuGetPushBranch = "master";
        NuGetPushRepoName = "https://github.com/AvaloniaUI/Avalonia.Native";

        var repoName = context.EnvironmentVariable("BUILD_REPOSITORY_URI");
        var repoBranch = context.EnvironmentVariable("BUILD_SOURCEBRANCHNAME");
        var repoTag = context.EnvironmentVariable("APPVEYOR_REPO_TAG");
        var repoTagName = context.EnvironmentVariable("APPVEYOR_REPO_TAG_NAME");
        var pullRequestTitle = context.EnvironmentVariable("SYSTEM_PULLREQUEST_SOURCEBRANCH");

        System.Console.WriteLine($"RepoName: {repoName}, RepoBranch: {repoBranch}, PR Title: {pullRequestTitle}/.");

        if (pullRequestTitle == null 
            && string.Compare(repoName, NuGetPushRepoName, StringComparison.OrdinalIgnoreCase) == 0
            && string.Compare(repoBranch, NuGetPushBranch, StringComparison.OrdinalIgnoreCase) == 0)
        {
            PushNuGet = true;
        }

        if (pullRequestTitle == null 
            && string.Compare(repoTag, "True", StringComparison.OrdinalIgnoreCase) == 0
            && repoTagName != null)
        {
            IsNugetRelease = true;
        }

        BuildProjects = new []
        {
            ( "./src", "Avalonia.Native" )
        };

        TestProjects = new (string path, string name) []
        {
        };

        PublishProjects = new (string path, string name, string framework, string runtime) []
        {
        };

        PackProjects = new []
        {
            ( "./src", "Avalonia.Native" )
        };
    }
}