using babbly_api_gateway.Models;
using babbly_api_gateway.Mocks;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class UserService : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly MockUserService _mockUserService;
    private readonly bool _useMockServices;

    public UserService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mockUserService = new MockUserService();
        _useMockServices = _configuration.GetValue<bool>("UseMockServices");
    }

    public async Task<User?> GetUserById(Guid id)
    {
        if (_useMockServices)
        {
            return await _mockUserService.GetUserById(id);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            // In a real app, we would log this exception
            Console.WriteLine($"Error in GetUserById: {ex.Message}");
        }

        return null;
    }

    public async Task<User?> GetUserByUsername(string username)
    {
        if (_useMockServices)
        {
            return await _mockUserService.GetUserByUsername(username);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/users/username/{username}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUserByUsername: {ex.Message}");
        }

        return null;
    }

    public async Task<List<User>> GetUsers()
    {
        if (_useMockServices)
        {
            return await _mockUserService.GetUsers();
        }

        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync("/api/users");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<User>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetUsers: {ex.Message}");
        }

        return new List<User>();
    }

    public async Task<List<User>> GetFollowers(Guid userId)
    {
        if (_useMockServices)
        {
            return await _mockUserService.GetFollowers(userId);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/users/{userId}/followers");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<User>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFollowers: {ex.Message}");
        }

        return new List<User>();
    }

    public async Task<List<User>> GetFollowing(Guid userId)
    {
        if (_useMockServices)
        {
            return await _mockUserService.GetFollowing(userId);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            var response = await client.GetAsync($"/api/users/{userId}/following");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<User>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<User>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFollowing: {ex.Message}");
        }

        return new List<User>();
    }
} 