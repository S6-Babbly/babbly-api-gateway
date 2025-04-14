using babbly_api_gateway.Models;
using babbly_api_gateway.Mocks;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class PostService : IPostService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly MockPostService _mockPostService;
    private readonly bool _useMockServices;

    public PostService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mockPostService = new MockPostService();
        _useMockServices = _configuration.GetValue<bool>("UseMockServices");
    }

    public async Task<List<Post>> GetPosts(int page = 1, int pageSize = 10)
    {
        if (_useMockServices)
        {
            return await _mockPostService.GetPosts(page, pageSize);
        }

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

    public async Task<Post?> GetPostById(Guid id)
    {
        if (_useMockServices)
        {
            return await _mockPostService.GetPostById(id);
        }

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

    public async Task<List<Post>> GetPostsByUserId(Guid userId, int page = 1, int pageSize = 10)
    {
        if (_useMockServices)
        {
            return await _mockPostService.GetPostsByUserId(userId, page, pageSize);
        }

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
            Console.WriteLine($"Error in GetPostsByUserId: {ex.Message}");
        }

        return new List<Post>();
    }
} 