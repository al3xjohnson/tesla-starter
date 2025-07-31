using System;
using FluentAssertions;
using TeslaStarter.Infrastructure.Configuration;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Configuration;

public class DescopeOptionsTests
{
    [Fact]
    public void Validate_WhenProjectIdIsNull_ThrowsInvalidOperationException()
    {
        var options = new DescopeOptions
        {
            ProjectId = null!,
            ManagementKey = "test-key"
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ProjectId is required in Descope configuration");
    }

    [Fact]
    public void Validate_WhenProjectIdIsEmpty_ThrowsInvalidOperationException()
    {
        var options = new DescopeOptions
        {
            ProjectId = string.Empty,
            ManagementKey = "test-key"
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ProjectId is required in Descope configuration");
    }

    [Fact]
    public void Validate_WhenProjectIdIsWhitespace_ThrowsInvalidOperationException()
    {
        var options = new DescopeOptions
        {
            ProjectId = "   ",
            ManagementKey = "test-key"
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ProjectId is required in Descope configuration");
    }

    [Fact]
    public void Validate_WhenManagementKeyIsNull_ThrowsInvalidOperationException()
    {
        var options = new DescopeOptions
        {
            ProjectId = "test-project-id",
            ManagementKey = null!
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ManagementKey is required in Descope configuration");
    }

    [Fact]
    public void Validate_WhenManagementKeyIsEmpty_ThrowsInvalidOperationException()
    {
        var options = new DescopeOptions
        {
            ProjectId = "test-project-id",
            ManagementKey = string.Empty
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ManagementKey is required in Descope configuration");
    }

    [Fact]
    public void Validate_WhenManagementKeyIsWhitespace_ThrowsInvalidOperationException()
    {
        var options = new DescopeOptions
        {
            ProjectId = "test-project-id",
            ManagementKey = "   "
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ManagementKey is required in Descope configuration");
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreValid_DoesNotThrow()
    {
        var options = new DescopeOptions
        {
            ProjectId = "test-project-id",
            ManagementKey = "test-key"
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void SectionName_ShouldBeDescope()
    {
        DescopeOptions.SectionName.Should().Be("Descope");
    }

    [Fact]
    public void DefaultValues_ShouldBeEmpty()
    {
        var options = new DescopeOptions();

        options.ProjectId.Should().BeEmpty();
        options.ManagementKey.Should().BeEmpty();
    }
}
