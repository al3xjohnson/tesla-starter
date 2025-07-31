namespace TeslaStarter.Domain.Tests.Users;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@example.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("user_name@example-domain.com")]
    [InlineData("123@example.com")]
    [InlineData("test@subdomain.example.com")]
    public void Create_WithValidEmail_ReturnsEmail(string validEmail)
    {
        // Act
        Email email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhitespace_ThrowsArgumentException(string invalidEmail)
    {
        // Act & Assert
        Action act = () => Email.Create(invalidEmail);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Email cannot be empty*")
            .And.ParamName.Should().Be("value");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test@.com")]
    [InlineData("test@example")]
    [InlineData("test @example.com")]
    [InlineData("test@@example.com")]
    [InlineData("test.example.com")]
    [InlineData("test@example@com")]
    public void Create_WithInvalidFormat_ThrowsArgumentException(string invalidEmail)
    {
        // Act & Assert
        Action act = () => Email.Create(invalidEmail);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid email format*")
            .And.ParamName.Should().Be("value");
    }

    [Fact]
    public void Create_WithUpperCaseEmail_ConvertsToLowerCase()
    {
        // Act
        Email email = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Create_WithWhitespace_TrimsEmail()
    {
        // Act
        Email email = Email.Create("  test@example.com  ");

        // Assert
        email.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void Equality_WithSameEmail_ReturnsTrue()
    {
        // Arrange
        Email email1 = Email.Create("test@example.com");
        Email email2 = Email.Create("test@example.com");

        // Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
        email1.GetHashCode().Should().Be(email2.GetHashCode());
    }

    [Fact]
    public void Equality_WithDifferentCase_ReturnsTrue()
    {
        // Arrange
        Email email1 = Email.Create("test@example.com");
        Email email2 = Email.Create("TEST@EXAMPLE.COM");

        // Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_WithDifferentEmail_ReturnsFalse()
    {
        // Arrange
        Email email1 = Email.Create("test1@example.com");
        Email email2 = Email.Create("test2@example.com");

        // Assert
        email1.Should().NotBe(email2);
        (email1 != email2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ReturnsEmailValue()
    {
        // Arrange
        Email email = Email.Create("test@example.com");

        // Act
        string result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitStringConversion_ReturnsEmailValue()
    {
        // Arrange
        Email email = Email.Create("test@example.com");

        // Act
        string result = email;

        // Assert
        result.Should().Be("test@example.com");
    }

}
