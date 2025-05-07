using babbly_api_gateway.Models;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class UserService : IUserService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public UserService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<User?> GetUserById(Guid id)
    {
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