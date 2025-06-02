using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using babbly_api_gateway.Services;
using babbly_api_gateway.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace babbly_api_gateway.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<ILogger<AuthService>> _mockLogger;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockLogger = new Mock<ILogger<AuthService>>();
            _authService = new AuthService(_mockHttpClientFactory.Object, _mockLogger.Object);
        }

        private Mock<HttpMessageHandler> CreateMockHttpMessageHandler(HttpStatusCode statusCode, HttpContent? content = null)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = content
                });
            return mockHttpMessageHandler;
        }

        [Fact]
        public async Task ValidateTokenAsync_ValidToken_ReturnsTrue()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.ValidateTokenAsync("valid_token");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_InvalidToken_ReturnsFalse()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.Unauthorized);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.ValidateTokenAsync("invalid_token");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_ServiceError_ReturnsFalse()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.ValidateTokenAsync("any_token");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_HttpRequestException_ReturnsFalseAndLogsError()
        {
            // Arrange
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Throws(new HttpRequestException("Simulated network error"));

            // Act
            var result = await _authService.ValidateTokenAsync("any_token");

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error validating token")),
                    It.IsAny<HttpRequestException>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task IsAuthorizedAsync_Authorized_ReturnsTrue()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.IsAuthorizedAsync("valid_token", "user1", "/resource", "read");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_Unauthorized_ReturnsFalse()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.Forbidden);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.IsAuthorizedAsync("valid_token", "user1", "/resource", "write");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_ServiceError_ReturnsFalse()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.IsAuthorizedAsync("any_token", "user1", "/resource", "read");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_HttpRequestException_ReturnsFalseAndLogsError()
        {
            // Arrange
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Throws(new HttpRequestException("Simulated network error"));

            // Act
            var result = await _authService.IsAuthorizedAsync("any_token", "user1", "/resource", "read");

            // Assert
            Assert.False(result);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error checking authorization")),
                    It.IsAny<HttpRequestException>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task GetUserInfoFromTokenAsync_ValidToken_ReturnsUserInfo()
        {
            // Arrange
            var expectedUserInfo = new babbly_api_gateway.Models.UserAuthInfo { UserId = "user1", Email = "test@example.com", Roles = new List<string> { "user" } };
            var userInfoJson = JsonSerializer.Serialize(expectedUserInfo);
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.OK, new StringContent(userInfoJson));
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.GetUserInfoFromTokenAsync("valid_token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUserInfo.UserId, result.UserId);
            Assert.Equal(expectedUserInfo.Email, result.Email);
            Assert.Equal(expectedUserInfo.Roles, result.Roles);
        }

        [Fact]
        public async Task GetUserInfoFromTokenAsync_ServiceError_ReturnsEmptyUserInfo()
        {
            // Arrange
            var mockHttpMessageHandler = CreateMockHttpMessageHandler(HttpStatusCode.InternalServerError);
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            // Act
            var result = await _authService.GetUserInfoFromTokenAsync("any_token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.UserId);
        }

        [Fact]
        public async Task GetUserInfoFromTokenAsync_HttpRequestException_ReturnsEmptyUserInfoAndLogsError()
        {
            // Arrange
            _mockHttpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Throws(new HttpRequestException("Simulated network error"));

            // Act
            var result = await _authService.GetUserInfoFromTokenAsync("any_token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(string.Empty, result.UserId);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error getting user info from token")),
                    It.IsAny<HttpRequestException>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
} 