using System;
using FluentAssertions;
using TeslaStarter.Infrastructure.Configuration;
using Xunit;

namespace TeslaStarter.Infrastructure.Tests.Configuration;

public class TeslaOptionsTests
{
    [Fact]
    public void Validate_WhenClientIdIsNull_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = null!,
            ClientSecret = "test-secret",
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ClientId is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenClientIdIsEmpty_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = string.Empty,
            ClientSecret = "test-secret",
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ClientId is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenClientIdIsWhitespace_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = "   ",
            ClientSecret = "test-secret",
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ClientId is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenClientSecretIsNull_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = "test-client-id",
            ClientSecret = null!,
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ClientSecret is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenClientSecretIsEmpty_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = "test-client-id",
            ClientSecret = string.Empty,
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ClientSecret is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenClientSecretIsWhitespace_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "   ",
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ClientSecret is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenRedirectUriIsNull_ThrowsInvalidOperationException()
    {
        var options = new TeslaOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            RedirectUri = null!
        };

        var act = () => options.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("RedirectUri is required in Tesla configuration");
    }

    [Fact]
    public void Validate_WhenAllPropertiesAreValid_DoesNotThrow()
    {
        var options = new TeslaOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-secret",
            RedirectUri = new Uri("https://test.com/callback")
        };

        var act = () => options.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void SectionName_ShouldBeTesla()
    {
        TeslaOptions.SectionName.Should().Be("Tesla");
    }

    [Fact]
    public void DefaultValues_ShouldBeEmpty()
    {
        var options = new TeslaOptions
        {
            RedirectUri = new Uri("https://test.com")
        };

        options.ClientId.Should().BeEmpty();
        options.ClientSecret.Should().BeEmpty();
    }
}
