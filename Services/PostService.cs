using babbly_api_gateway.Models;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class PostService : IPostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public PostService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<Post?> GetPostById(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PostService");
            var response = await client.GetAsync($"/api/posts/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Post>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPostById: {ex.Message}");
        }

        return null;
    }

    public async Task<List<Post>> GetPosts(int page = 1, int pageSize = 10)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PostService");
            var response = await client.GetAsync($"/api/posts?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Post>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Post>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPosts: {ex.Message}");
        }

        return new List<Post>();
    }

    public async Task<List<Post>> GetPostsByUserId(Guid userId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PostService");
            var response = await client.GetAsync($"/api/posts/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Post>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Post>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPostsByUserId: {ex.Message}");
        }

        return new List<Post>();
    }

    public async Task<List<Post>> GetPostsByUserId(Guid userId, int page, int pageSize)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PostService");
            var response = await client.GetAsync($"/api/posts/user/{userId}?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Post>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Post>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetPostsByUserId with pagination: {ex.Message}");
        }

        return new List<Post>();
    }

    public async Task<List<Post>> GetFeed(Guid userId, int page = 1, int pageSize = 10)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PostService");
            var response = await client.GetAsync($"/api/posts/feed/{userId}?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Post>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Post>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetFeed: {ex.Message}");
        }

        return new List<Post>();
    }
} 