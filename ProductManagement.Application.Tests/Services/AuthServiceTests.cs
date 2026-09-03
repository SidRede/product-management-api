using Moq;
using ProductManagement.Application.DTOs.Authentication;
using ProductManagement.Application.Interfaces.Repositories;
using ProductManagement.Application.Interfaces.Services;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;
using Xunit;

namespace ProductManagement.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;

    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _refreshTokenRepositoryMock =
            new Mock<IRefreshTokenRepository>();

        _jwtTokenServiceMock =
            new Mock<IJwtTokenService>();

        _unitOfWorkMock
            .Setup(x => x.Users)
            .Returns(_userRepositoryMock.Object);

        _unitOfWorkMock
            .Setup(x => x.RefreshTokens)
            .Returns(_refreshTokenRepositoryMock.Object);

        _authService = new AuthService(
            _unitOfWorkMock.Object,
            _jwtTokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WhenNewUserProvided_ReturnsAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "Test User",
            Email = "test@example.com",
            Password = "Password@123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync((User?)null);

        _userRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<User>()))
            .Returns("test-access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(registerDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-access-token", result!.AccessToken);
        Assert.Equal("test-refresh-token", result.RefreshToken);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.Is<User>(u =>
                u.UserName == registerDto.UserName &&
                u.Email == registerDto.Email &&
                u.Role == "User")),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            UserName = "Test User",
            Email = "existing@example.com",
            Password = "Password@123"
        };

        var existingUser = new User
        {
            Id = 1,
            UserName = "Existing User",
            Email = "existing@example.com",
            PasswordHash = "hashed-password"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(registerDto.Email))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<BadRequestException>(
                () => _authService.RegisterAsync(registerDto));

        Assert.Contains(
            "already exists",
            exception.Message);

        _userRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsAreValid_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "Password@123"
        };

        var user = new User
        {
            Id = 1,
            UserName = "Test User",
            Email = "test@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("Password@123"),
            Role = "User"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("test-access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("test-refresh-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test-access-token", result!.AccessToken);
        Assert.Equal("test-refresh-token", result.RefreshToken);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateAccessToken(user),
            Times.Once);

        _jwtTokenServiceMock.Verify(
            x => x.GenerateRefreshToken(),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "unknown@example.com",
            Password = "Password@123"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.LoginAsync(loginDto));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }


    [Fact]
    public async Task LoginAsync_WhenPasswordIsInvalid_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Id = 1,
            UserName = "Test User",
            Email = "test@example.com",
            PasswordHash =
                BCrypt.Net.BCrypt.HashPassword("CorrectPassword"),
            Role = "User"
        };

        _userRepositoryMock
            .Setup(x => x.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(user);

        // Act & Assert
        var exception =
            await Assert.ThrowsAsync<UnauthorizedException>(
                () => _authService.LoginAsync(loginDto));

        Assert.Equal(
            "Invalid email or password.",
            exception.Message);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenIsValid_RotatesTokenAndReturnsNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "old-refresh-token"
        };

        var user = new User
        {
            Id = 1,
            UserName = "Test User",
            Email = "test@example.com",
            Role = "User"
        };

        var oldRefreshToken = new RefreshToken
        {
            Id = 1,
            Token = "old-refresh-token",
            UserId = 1,
            User = user,
            CreatedOn = DateTime.UtcNow.AddDays(-1),
            ExpiresOn = DateTime.UtcNow.AddDays(5),
            IsRevoked = false
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken))
            .ReturnsAsync(oldRefreshToken);

        _refreshTokenRepositoryMock
            .Setup(x => x.Update(oldRefreshToken));

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(user))
            .Returns("new-access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");

        _refreshTokenRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result =
            await _authService.RefreshTokenAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(
            "new-access-token",
            result!.AccessToken);

        Assert.Equal(
            "new-refresh-token",
            result.RefreshToken);

        Assert.True(oldRefreshToken.IsRevoked);

        _refreshTokenRepositoryMock.Verify(
            x => x.Update(oldRefreshToken),
            Times.Once);

        _refreshTokenRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<RefreshToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenDoesNotExist_ReturnsNull()
    {
        // Arrange
        var request = new RefreshTokenRequestDto
        {
            RefreshToken = "invalid-token"
        };

        _refreshTokenRepositoryMock
            .Setup(x => x.GetByTokenAsync(request.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result =
            await _authService.RefreshTokenAsync(request);

        // Assert
        Assert.Null(result);

        _refreshTokenRepositoryMock.Verify(
            x => x.Update(It.IsAny<RefreshToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);
    }
}