using babbly_api_gateway.Models;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class CommentService : ICommentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public CommentService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<Comment?> GetCommentById(Guid id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CommentService");
            var response = await client.GetAsync($"/api/comments/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Comment>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCommentById: {ex.Message}");
        }

        return null;
    }

    public async Task<List<Comment>> GetCommentsByPostId(Guid postId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CommentService");
            var response = await client.GetAsync($"/api/comments/post/{postId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Comment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Comment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCommentsByPostId: {ex.Message}");
        }

        return new List<Comment>();
    }

    public async Task<List<Comment>> GetCommentsByPostId(Guid postId, int page, int pageSize)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CommentService");
            var response = await client.GetAsync($"/api/comments/post/{postId}?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Comment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Comment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCommentsByPostId with pagination: {ex.Message}");
        }

        return new List<Comment>();
    }

    public async Task<List<Comment>> GetCommentsByUserId(string userId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CommentService");
            var response = await client.GetAsync($"/api/comments/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Comment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Comment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCommentsByUserId: {ex.Message}");
        }

        return new List<Comment>();
    }

    public async Task<List<Comment>> GetCommentsByUserId(string userId, int page, int pageSize)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CommentService");
            var response = await client.GetAsync($"/api/comments/user/{userId}?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Comment>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Comment>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCommentsByUserId with pagination: {ex.Message}");
        }

        return new List<Comment>();
    }
} 