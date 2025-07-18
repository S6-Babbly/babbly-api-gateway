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

    public async Task<User?> GetUserById(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("UserService");
            // Use auth0 endpoint since posts contain Auth0 user IDs like auth0|12345
            var requestUrl = $"/api/users/auth0/{id}";
            Console.WriteLine($"🔍 UserService: Fetching user data for ID: {id} from URL: {requestUrl}");
            
            var response = await client.GetAsync(requestUrl);
            Console.WriteLine($"📡 UserService: Response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"📄 UserService: Response content: {content}");
                
                var user = JsonSerializer.Deserialize<User>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                Console.WriteLine($"✅ UserService: Successfully parsed user: {user?.Username} (Auth0: {user?.Auth0Id})");
                return user;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"❌ UserService: Failed to fetch user. Status: {response.StatusCode}, Content: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            // In a real app, we would log this exception
            Console.WriteLine($"💥 UserService: Error in GetUserById for {id}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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

    public async Task<List<User>> GetFollowers(string userId)
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

    public async Task<List<User>> GetFollowing(string userId)
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