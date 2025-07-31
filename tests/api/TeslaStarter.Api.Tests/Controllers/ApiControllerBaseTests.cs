using TeslaStarter.Api.Controllers;

namespace TeslaStarter.Api.Tests.Controllers;

public sealed class ApiControllerBaseTests
{
    private readonly TestApiController _controller;
    private readonly Mock<ISender> _mediatorMock;

    public ApiControllerBaseTests()
    {
        _mediatorMock = new();
        _controller = new TestApiController();

        // Set up HttpContext with mocked services
        ServiceCollection services = new();
        services.AddSingleton(_mediatorMock.Object);

        DefaultHttpContext httpContext = new()
        {
            RequestServices = services.BuildServiceProvider()
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public void Mediator_LazilyInitialized_ReturnsSameInstance()
    {
        // Act
        ISender mediator1 = _controller.GetMediator();
        ISender mediator2 = _controller.GetMediator();

        // Assert
        mediator1.Should().BeSameAs(mediator2);
        mediator1.Should().BeSameAs(_mediatorMock.Object);
    }

    [Fact]
    public void HandleResult_NonNullResult_ReturnsOk()
    {
        // Arrange
        TestDto result = new() { Id = 1, Name = "Test" };

        // Act
        ActionResult<TestDto> actionResult = _controller.TestHandleResult(result);

        // Assert
        OkObjectResult okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(result);
    }

    [Fact]
    public void HandleResult_NullResult_ReturnsNotFound()
    {
        // Arrange
        TestDto? result = null;

        // Act
        ActionResult<TestDto> actionResult = _controller.TestHandleResult(result);

        // Assert
        actionResult.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void HandleResult_BoolTrue_ReturnsNoContent()
    {
        // Act
        ActionResult actionResult = _controller.TestHandleResultBool(true);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void HandleResult_BoolFalse_ReturnsNotFound()
    {
        // Act
        ActionResult actionResult = _controller.TestHandleResultBool(false);

        // Assert
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public void Created_WithLocation_ReturnsCreatedResult()
    {
        // Arrange
        TestDto result = new() { Id = 1, Name = "Test" };
        string location = "api/v1/test/1";

        // Act
        ActionResult<TestDto> actionResult = _controller.TestCreatedWithLocation(result, location);

        // Assert
        CreatedResult createdResult = actionResult.Result.Should().BeOfType<CreatedResult>().Subject;
        createdResult.Location.Should().Be(location);
        createdResult.Value.Should().BeEquivalentTo(result);
    }

    [Fact]
    public void Created_WithoutLocation_ReturnsCreatedAtAction()
    {
        // Arrange
        TestDto result = new() { Id = 1, Name = "Test" };

        // Act
        ActionResult<TestDto> actionResult = _controller.TestCreatedWithoutLocation(result);

        // Assert
        CreatedAtActionResult createdResult = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.Value.Should().BeEquivalentTo(result);
    }

    // Test controller to expose protected methods
    private sealed class TestApiController : ApiControllerBase
    {
        public ISender GetMediator() => Mediator;

        public ActionResult<TestDto> TestHandleResult(TestDto? result) => HandleResult(result);

        public ActionResult TestHandleResultBool(bool result) => HandleResult(result);

        public ActionResult<TestDto> TestCreatedWithLocation(TestDto result, string location)
            => Created(result, location);

        public ActionResult<TestDto> TestCreatedWithoutLocation(TestDto result)
            => Created(result);
    }

    private sealed class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
