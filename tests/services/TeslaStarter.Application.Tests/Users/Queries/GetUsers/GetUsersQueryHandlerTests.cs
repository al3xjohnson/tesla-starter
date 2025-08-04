using System.Diagnostics.CodeAnalysis;
using AutoMapper;
using FluentAssertions;
using Moq;
using TeslaStarter.Application.Common.Interfaces;
using TeslaStarter.Application.Users.DTOs;
using TeslaStarter.Application.Users.Queries.GetUsers;
using TeslaStarter.Domain.Users;
using Xunit;

namespace TeslaStarter.Application.Tests.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetUsersQueryHandler _handler;

    public GetUsersQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetUsersQueryHandler(_userRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoUsers_ReturnsEmptyList()
    {
        // Arrange
        GetUsersQuery query = new();
        List<User> users = [];
        List<UserDto> userDtos = [];

        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mapperMock
            .Setup(x => x.Map<List<UserDto>>(users))
            .Returns(userDtos);

        // Act
        List<UserDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _userRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<List<UserDto>>(users), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUsersExist_ReturnsMappedUserDtos()
    {
        // Arrange
        GetUsersQuery query = new();

        List<User> users =
        [
            User.Create("external1", "user1@example.com", "User One"),
            User.Create("external2", "user2@example.com", "User Two"),
            User.Create("external3", "user3@example.com", null)
        ];

        List<UserDto> userDtos =
        [
            new UserDto { Id = Guid.NewGuid(), Email = "user1@example.com", DisplayName = "User One" },
            new UserDto { Id = Guid.NewGuid(), Email = "user2@example.com", DisplayName = "User Two" },
            new UserDto { Id = Guid.NewGuid(), Email = "user3@example.com", DisplayName = null }
        ];

        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mapperMock
            .Setup(x => x.Map<List<UserDto>>(users))
            .Returns(userDtos);

        // Act
        List<UserDto> result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeEquivalentTo(userDtos);
        _userRepositoryMock.Verify(x => x.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(x => x.Map<List<UserDto>>(users), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_PassesTokenToRepository()
    {
        // Arrange
        GetUsersQuery query = new();
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        List<User> users = [];
        List<UserDto> userDtos = [];

        _userRepositoryMock
            .Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mapperMock
            .Setup(x => x.Map<List<UserDto>>(users))
            .Returns(userDtos);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _userRepositoryMock.Verify(x => x.GetAllAsync(cancellationToken), Times.Once);
    }
}
