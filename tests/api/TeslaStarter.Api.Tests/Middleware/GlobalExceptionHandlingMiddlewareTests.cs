using Microsoft.AspNetCore.Http.Features;

namespace TeslaStarter.Api.Tests.Middleware;

public sealed class GlobalExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlingMiddleware>> _loggerMock;
    private readonly Mock<IHostEnvironment> _environmentMock;

    public GlobalExceptionHandlingMiddlewareTests()
    {
        _loggerMock = new();
        _environmentMock = new();
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        // Arrange
        DefaultHttpContext context = new();
        bool nextCalled = false;

        Task next(HttpContext ctx)
        {
            nextCalled = true;
            return Task.CompletedTask;
        }

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_ValidationException_ReturnsBadRequest()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        Dictionary<string, string[]> errors = new()
        {
            ["Email"] = ["Email is required", "Email is invalid"],
            ["Name"] = ["Name is required"]
        };
        ValidationException exception = new(errors.SelectMany(kvp =>
            kvp.Value.Select(v => new FluentValidation.Results.ValidationFailure(kvp.Key, v))));

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().StartWith("application/json");

        string responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("One or more validation errors occurred.");
        responseBody.Should().Contain("Email is required");
        responseBody.Should().Contain("Email is invalid");
        responseBody.Should().Contain("Name is required");
    }

    [Fact]
    public async Task InvokeAsync_NotFoundException_ReturnsNotFound()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        NotFoundException exception = new("User", Guid.NewGuid());

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        context.Response.ContentType.Should().StartWith("application/json");

        string responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("Resource not found");
        // The JSON contains Unicode escape sequence for quotes
        responseBody.Should().Contain("Entity")
                   .And.Contain("User")
                   .And.Contain("was not found");
    }

    [Fact]
    public async Task InvokeAsync_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        UnauthorizedAccessException exception = new();

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().StartWith("application/json");

        string responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("Unauthorized");
        responseBody.Should().Contain("You are not authorized to access this resource");
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsInternalServerError_InDevelopment()
    {
        // Arrange
        _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
        DefaultHttpContext context = CreateHttpContext();
        InvalidOperationException exception = new("Something went wrong");

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/json");

        string responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("An error occurred while processing your request.");
        responseBody.Should().Contain("Something went wrong"); // Detailed message in development
        responseBody.Should().Contain("stackTrace"); // Stack trace in development
    }

    [Fact]
    public async Task InvokeAsync_GenericException_ReturnsInternalServerError_InProduction()
    {
        // Arrange
        _environmentMock.Setup(x => x.EnvironmentName).Returns("Production");
        DefaultHttpContext context = CreateHttpContext();
        InvalidOperationException exception = new("Something went wrong");

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().StartWith("application/json");

        string responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("An error occurred while processing your request.");
        responseBody.Should().NotContain("Something went wrong"); // No detailed message
        responseBody.Should().NotContain("stackTrace"); // No stack trace in production
    }

    [Fact]
    public async Task InvokeAsync_AllExceptions_LogsError()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        InvalidOperationException exception = new("Test error");

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _loggerMock.Verify(x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("An unhandled exception occurred")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_Response_IncludesTraceId()
    {
        // Arrange
        DefaultHttpContext context = CreateHttpContext();
        context.TraceIdentifier = "test-trace-id";
        NotFoundException exception = new("User", 123);

        Task next(HttpContext ctx) => throw exception;

        GlobalExceptionHandlingMiddleware middleware = new(next, _loggerMock.Object, _environmentMock.Object);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        string responseBody = await ReadResponseBody(context);
        responseBody.Should().Contain("test-trace-id");
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        context.Features.Set<IHttpResponseFeature>(new HttpResponseFeature());
        return context;
    }

    private static async Task<string> ReadResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using StreamReader reader = new(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    private sealed class HttpResponseFeature : IHttpResponseFeature
    {
        public Stream Body { get; set; } = new MemoryStream();
        public bool HasStarted => false;
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public string? ReasonPhrase { get; set; }
        public int StatusCode { get; set; }
        public void OnCompleted(Func<object, Task> callback, object state) { }
        public void OnStarting(Func<object, Task> callback, object state) { }
    }
}
