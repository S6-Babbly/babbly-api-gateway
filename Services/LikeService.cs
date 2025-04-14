using babbly_api_gateway.Models;
using babbly_api_gateway.Mocks;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class LikeService : ILikeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly MockLikeService _mockLikeService;
    private readonly bool _useMockServices;

    public LikeService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _mockLikeService = new MockLikeService();
        _useMockServices = _configuration.GetValue<bool>("UseMockServices");
    }

    public async Task<List<Like>> GetLikesByPostId(Guid postId, int page = 1, int pageSize = 50)
    {
        if (_useMockServices)
        {
            return await _mockLikeService.GetLikesByPostId(postId, page, pageSize);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("LikeService");
            var response = await client.GetAsync($"/api/likes/post/{postId}?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Like>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Like>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetLikesByPostId: {ex.Message}");
        }

        return new List<Like>();
    }

    public async Task<List<Like>> GetLikesByUserId(Guid userId, int page = 1, int pageSize = 50)
    {
        if (_useMockServices)
        {
            return await _mockLikeService.GetLikesByUserId(userId, page, pageSize);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("LikeService");
            var response = await client.GetAsync($"/api/likes/user/{userId}?page={page}&pageSize={pageSize}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Like>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Like>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetLikesByUserId: {ex.Message}");
        }

        return new List<Like>();
    }

    public async Task<bool> HasUserLikedPost(Guid userId, Guid postId)
    {
        if (_useMockServices)
        {
            return await _mockLikeService.HasUserLikedPost(userId, postId);
        }

        try
        {
            var client = _httpClientFactory.CreateClient("LikeService");
            var response = await client.GetAsync($"/api/likes/check?userId={userId}&postId={postId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<bool>(content);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in HasUserLikedPost: {ex.Message}");
        }

        return false;
    }
} 