using babbly_api_gateway.Models;
using System.Text.Json;
using System.Text;

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

    public async Task<List<Post>> GetPostsByUserId(string userId)
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

    public async Task<List<Post>> GetPostsByUserId(string userId, int page, int pageSize)
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

    public async Task<List<Post>> GetFeed(string userId, int page = 1, int pageSize = 10)
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

    public async Task<Post> CreatePost(object postData)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("PostService");
            var json = JsonSerializer.Serialize(postData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await client.PostAsync("/api/posts", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var createdPost = JsonSerializer.Deserialize<Post>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return createdPost ?? throw new Exception("Failed to deserialize created post");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Post service returned {response.StatusCode}: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreatePost: {ex.Message}");
            throw;
        }
    }
} 