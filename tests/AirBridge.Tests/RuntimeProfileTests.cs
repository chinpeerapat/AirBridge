using AirBridge.Core;

namespace AirBridge.Tests;

public sealed class RuntimeProfileTests
{
    [Fact]
    public void ProductionKeepsExistingStorageAndCredentialTarget()
    {
        var root = Path.Combine(Path.GetTempPath(), "airbridge-user");
        Assert.Equal(Path.Combine(root, "AirBridge"), RuntimeProfile.ResolveDataDirectory(null, root));
        Assert.Equal("AirBridge/OpenAI API Key", RuntimeProfile.ResolveCredentialTarget(null));
    }

    [Fact]
    public void WorktreeProfilesHaveSeparateStableCredentialTargets()
    {
        var first = Path.Combine(Path.GetTempPath(), "airbridge-tree-a");
        var second = Path.Combine(Path.GetTempPath(), "airbridge-tree-b");
        Assert.NotEqual(RuntimeProfile.ResolveCredentialTarget(first), RuntimeProfile.ResolveCredentialTarget(second));
        Assert.Equal(RuntimeProfile.ResolveCredentialTarget(first), RuntimeProfile.ResolveCredentialTarget(first + Path.DirectorySeparatorChar));
        Assert.NotEqual(RuntimeProfile.DefaultCredentialTarget, RuntimeProfile.ResolveCredentialTarget(first));
    }

    [Fact]
    public void PortableMarkerKeepsProfileBesideTheApplication()
    {
        var app = Path.Combine(Path.GetTempPath(), "airbridge-portable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(app);
        try
        {
            Assert.Null(RuntimeProfile.ResolvePortableDirectory(app));
            File.WriteAllText(Path.Combine(app, RuntimeProfile.PortableMarkerFileName), string.Empty);
            var data = RuntimeProfile.ResolvePortableDirectory(app);
            Assert.Equal(Path.Combine(app, "Data"), data);
            Assert.NotEqual(RuntimeProfile.DefaultCredentialTarget, RuntimeProfile.ResolveCredentialTarget(data));
        }
        finally { Directory.Delete(app, true); }
    }

    [Fact]
    public void PerUserInstallIsRecognisedOnlyInItsInstallFolder()
    {
        var localAppData = Path.Combine(Path.GetTempPath(), "airbridge-local");
        var installed = Path.Combine(localAppData, "Programs", "AirBridge");
        Assert.True(RuntimeProfile.IsPerUserInstallDirectory(installed + Path.DirectorySeparatorChar, localAppData));
        Assert.True(RuntimeProfile.IsPerUserInstallDirectory(installed.ToUpperInvariant(), localAppData));
        Assert.False(RuntimeProfile.IsPerUserInstallDirectory(Path.Combine(localAppData, "Programs", "Other"), localAppData));
        Assert.False(RuntimeProfile.IsPerUserInstallDirectory(Path.Combine(Path.GetTempPath(), "Program Files", "AirBridge"), localAppData));
    }

    [Fact]
    public void ExplicitSettingsPathsStayIndependentOfProfiles()
    {
        var firstPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "settings.json");
        try
        {
            var store = new SettingsStore(firstPath);
            store.Save(new AirBridgeSettings { DefaultReceiverName = "Fixture speaker" });
            Assert.Equal(firstPath, store.Path);
            Assert.Equal("Fixture speaker", store.Load().DefaultReceiverName);
        }
        finally { Directory.Delete(Path.GetDirectoryName(firstPath)!, true); }
    }
}
