namespace TeslaStarter.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        // Arrange
        IEnumerable<IValidator<TestCommand>> validators = Enumerable.Empty<IValidator<TestCommand>>();
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();
        TestResponse expectedResponse = new() { Value = "Success" };

        Mock<RequestHandlerDelegate<TestResponse>> next = new();
        next.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        TestResponse result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        next.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        // Arrange
        Mock<IValidator<TestCommand>> validator = new();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        IValidator<TestCommand>[] validators = [validator.Object];
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();
        TestResponse expectedResponse = new() { Value = "Success" };

        Mock<RequestHandlerDelegate<TestResponse>> next = new();
        next.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        TestResponse result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        validator.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        next.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        // Arrange
        List<ValidationFailure> failures =
        [
            new ValidationFailure("Property1", "Error message 1"),
            new ValidationFailure("Property2", "Error message 2")
        ];

        Mock<IValidator<TestCommand>> validator = new();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures));

        IValidator<TestCommand>[] validators = [validator.Object];
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();

        Mock<RequestHandlerDelegate<TestResponse>> next = new();

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => behavior.Handle(request, next.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors["Property1"].Should().Contain("Error message 1");
        exception.Errors["Property2"].Should().Contain("Error message 2");

        validator.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        next.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_MultipleValidators_AllValidatorsRun()
    {
        // Arrange
        Mock<IValidator<TestCommand>> validator1 = new();
        validator1.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Mock<IValidator<TestCommand>> validator2 = new();
        validator2.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        IValidator<TestCommand>[] validators = [validator1.Object, validator2.Object];
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();
        TestResponse expectedResponse = new() { Value = "Success" };

        Mock<RequestHandlerDelegate<TestResponse>> next = new();
        next.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        TestResponse result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        validator1.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator2.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        next.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleValidatorsWithErrors_CombinesAllErrors()
    {
        // Arrange
        List<ValidationFailure> failures1 =
        [
            new ValidationFailure("Property1", "Error from validator 1")
        ];

        List<ValidationFailure> failures2 =
        [
            new ValidationFailure("Property2", "Error from validator 2"),
            new ValidationFailure("Property1", "Another error for Property1")
        ];

        Mock<IValidator<TestCommand>> validator1 = new();
        validator1.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures1));

        Mock<IValidator<TestCommand>> validator2 = new();
        validator2.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures2));

        IValidator<TestCommand>[] validators = [validator1.Object, validator2.Object];
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();

        Mock<RequestHandlerDelegate<TestResponse>> next = new();

        // Act & Assert
        Application.Common.Exceptions.ValidationException exception = await Assert.ThrowsAsync<Application.Common.Exceptions.ValidationException>(
            () => behavior.Handle(request, next.Object, CancellationToken.None));

        exception.Errors.Should().HaveCount(2);
        exception.Errors["Property1"].Should().HaveCount(2);
        exception.Errors["Property1"].Should().Contain("Error from validator 1");
        exception.Errors["Property1"].Should().Contain("Another error for Property1");
        exception.Errors["Property2"].Should().ContainSingle();
        exception.Errors["Property2"].Should().Contain("Error from validator 2");

        next.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidatorReturnsNoErrors_CallsNext()
    {
        // Arrange
        Mock<IValidator<TestCommand>> validator = new();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // No errors

        IValidator<TestCommand>[] validators = [validator.Object];
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();
        TestResponse expectedResponse = new() { Value = "Success" };

        Mock<RequestHandlerDelegate<TestResponse>> next = new();
        next.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        TestResponse result = await behavior.Handle(request, next.Object, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
        next.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_CancellationRequested_PassesCancellationToken()
    {
        // Arrange
        CancellationToken cancellationToken = new();
        Mock<IValidator<TestCommand>> validator = new();
        validator.Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), cancellationToken))
            .ReturnsAsync(new ValidationResult());

        IValidator<TestCommand>[] validators = [validator.Object];
        ValidationBehavior<TestCommand, TestResponse> behavior = new(validators);
        TestCommand request = new();
        TestResponse expectedResponse = new() { Value = "Success" };

        Mock<RequestHandlerDelegate<TestResponse>> next = new();
        next.Setup(x => x()).ReturnsAsync(expectedResponse);

        // Act
        await behavior.Handle(request, next.Object, cancellationToken);

        // Assert
        validator.Verify(x => x.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), cancellationToken), Times.Once);
    }

    // Test classes
    public sealed class TestCommand : IRequest<TestResponse>
    {
        public string? Property1 { get; set; }
        public string? Property2 { get; set; }
    }

    public sealed class TestResponse
    {
        public string Value { get; set; } = string.Empty;
    }
}
