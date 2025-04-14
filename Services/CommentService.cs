using babbly_api_gateway.Models;
using babbly_api_gateway.Mocks;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class CommentService : ICommentService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly MockCommentService _mockCommentService;
    private readonly bool _useMockServices;

    public CommentService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mockCommentService = new MockCommentService();
        _useMockServices = _configuration.GetValue<bool>("UseMockServices");
    }

    public async Task<List<Comment>> GetCommentsByPostId(Guid postId, int page = 1, int pageSize = 20)
    {
        if (_useMockServices)
        {
            return await _mockCommentService.GetCommentsByPostId(postId, page, pageSize);
        }

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
            Console.WriteLine($"Error in GetCommentsByPostId: {ex.Message}");
        }

        return new List<Comment>();
    }

    public async Task<List<Comment>> GetCommentsByUserId(Guid userId, int page = 1, int pageSize = 20)
    {
        if (_useMockServices)
        {
            return await _mockCommentService.GetCommentsByUserId(userId, page, pageSize);
        }

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
            Console.WriteLine($"Error in GetCommentsByUserId: {ex.Message}");
        }

        return new List<Comment>();
    }

    public async Task<Comment?> GetCommentById(Guid id)
    {
        if (_useMockServices)
        {
            return await _mockCommentService.GetCommentById(id);
        }

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
} 