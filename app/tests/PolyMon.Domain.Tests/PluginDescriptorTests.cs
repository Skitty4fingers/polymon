using FluentAssertions;
using PolyMon.Domain.Plugins;

namespace PolyMon.Domain.Tests;

public class PluginDescriptorTests
{
    [Fact]
    public void DefaultValues_AreSet()
    {
        var descriptor = new PluginDescriptor();
        descriptor.TypeKey.Should().BeEmpty();
        descriptor.DisplayName.Should().BeEmpty();
        descriptor.Parameters.Should().BeEmpty();
    }

    [Fact]
    public void Parameters_CanBeAdded()
    {
        var descriptor = new PluginDescriptor
        {
            TypeKey = "CPU",
            DisplayName = "CPU Monitor",
            Parameters =
            [
                new PluginParameter { Name = "WarnThreshold", Type = "decimal", Default = 80, Required = true },
                new PluginParameter { Name = "ComputerName", Type = "string", Default = "localhost" }
            ]
        };

        descriptor.Parameters.Should().HaveCount(2);
        descriptor.Parameters[0].Name.Should().Be("WarnThreshold");
        descriptor.Parameters[0].Required.Should().BeTrue();
        descriptor.Parameters[1].Default.Should().Be("localhost");
    }

    [Fact]
    public void PluginParameter_DefaultType_IsString()
    {
        var param = new PluginParameter { Name = "Host" };
        param.Type.Should().Be("string");
        param.Required.Should().BeFalse();
    }
}
