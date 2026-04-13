using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using PolyMon.Infrastructure.Plugins;

namespace PolyMon.Application.Tests;

public class PluginScannerTests : IDisposable
{
    private readonly string _tempDir;

    public PluginScannerTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private PluginScanner CreateScanner() =>
        new(_tempDir, NullLogger<PluginScanner>.Instance);

    [Fact]
    public void Reload_EmptyDirectory_LoadsZeroDescriptors()
    {
        var scanner = CreateScanner();
        scanner.Reload();
        scanner.Descriptors.Should().BeEmpty();
    }

    [Fact]
    public void Reload_ValidPluginPair_LoadsDescriptor()
    {
        File.WriteAllText(Path.Combine(_tempDir, "cpu.plugin.json"), """
            {
              "TypeKey": "CPU",
              "DisplayName": "CPU Monitor",
              "Description": "Monitors CPU",
              "Parameters": [
                { "Name": "WarnThreshold", "Type": "decimal", "Default": 80, "Required": true }
              ]
            }
            """);
        File.WriteAllText(Path.Combine(_tempDir, "cpu.ps1"), "# placeholder");

        var scanner = CreateScanner();
        scanner.Reload();

        scanner.Descriptors.Should().HaveCount(1);
        var descriptor = scanner.Descriptors[0];
        descriptor.TypeKey.Should().Be("CPU");
        descriptor.DisplayName.Should().Be("CPU Monitor");
        descriptor.Parameters.Should().HaveCount(1);
        descriptor.Parameters[0].Name.Should().Be("WarnThreshold");
        descriptor.Parameters[0].Required.Should().BeTrue();
        descriptor.ScriptPath.Should().EndWith("cpu.ps1");
    }

    [Fact]
    public void Reload_MissingScriptFile_SkipsDescriptor()
    {
        File.WriteAllText(Path.Combine(_tempDir, "disk.plugin.json"), """
            { "TypeKey": "DISK", "DisplayName": "Disk Monitor", "Parameters": [] }
            """);
        // No disk.ps1 file

        var scanner = CreateScanner();
        scanner.Reload();

        scanner.Descriptors.Should().BeEmpty();
    }

    [Fact]
    public void Reload_InvalidJson_SkipsDescriptor()
    {
        File.WriteAllText(Path.Combine(_tempDir, "bad.plugin.json"), "not valid json {{{");
        File.WriteAllText(Path.Combine(_tempDir, "bad.ps1"), "# placeholder");

        var scanner = CreateScanner();
        // Should not throw
        scanner.Reload();
        scanner.Descriptors.Should().BeEmpty();
    }

    [Fact]
    public void GetByKey_ExistingKey_ReturnsDescriptor()
    {
        File.WriteAllText(Path.Combine(_tempDir, "ping.plugin.json"), """
            { "TypeKey": "PING", "DisplayName": "Ping Monitor", "Parameters": [] }
            """);
        File.WriteAllText(Path.Combine(_tempDir, "ping.ps1"), "# placeholder");

        var scanner = CreateScanner();
        scanner.Reload();

        var result = scanner.GetByKey("PING");
        result.Should().NotBeNull();
        result!.DisplayName.Should().Be("Ping Monitor");
    }

    [Fact]
    public void GetByKey_CaseInsensitive_ReturnsDescriptor()
    {
        File.WriteAllText(Path.Combine(_tempDir, "ping.plugin.json"), """
            { "TypeKey": "PING", "DisplayName": "Ping Monitor", "Parameters": [] }
            """);
        File.WriteAllText(Path.Combine(_tempDir, "ping.ps1"), "# placeholder");

        var scanner = CreateScanner();
        scanner.Reload();

        scanner.GetByKey("ping").Should().NotBeNull();
        scanner.GetByKey("Ping").Should().NotBeNull();
    }

    [Fact]
    public void GetByKey_NonexistentKey_ReturnsNull()
    {
        var scanner = CreateScanner();
        scanner.Reload();
        scanner.GetByKey("NONEXISTENT").Should().BeNull();
    }

    [Fact]
    public void Reload_NonexistentDirectory_LoadsZeroDescriptors()
    {
        var scanner = new PluginScanner(
            Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid()),
            NullLogger<PluginScanner>.Instance);

        scanner.Reload();
        scanner.Descriptors.Should().BeEmpty();
    }

    [Fact]
    public void Reload_MultipleValidPlugins_LoadsAll()
    {
        foreach (var key in new[] { "cpu", "disk", "ping" })
        {
            File.WriteAllText(Path.Combine(_tempDir, $"{key}.plugin.json"),
                $$"""{ "TypeKey": "{{key.ToUpper()}}", "DisplayName": "{{key}} Monitor", "Parameters": [] }""");
            File.WriteAllText(Path.Combine(_tempDir, $"{key}.ps1"), "# placeholder");
        }

        var scanner = CreateScanner();
        scanner.Reload();

        scanner.Descriptors.Should().HaveCount(3);
    }
}
