using babbly_api_gateway.Models;
using System.Text.Json;

namespace babbly_api_gateway.Services;

public class LikeService : ILikeService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public LikeService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<Like>> GetLikesByPostId(Guid postId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("LikeService");
            var response = await client.GetAsync($"/api/likes/post/{postId}");

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

    public async Task<List<Like>> GetLikesByPostId(Guid postId, int page, int pageSize)
    {
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
            Console.WriteLine($"Error in GetLikesByPostId with pagination: {ex.Message}");
        }

        return new List<Like>();
    }

    public async Task<List<Like>> GetLikesByUserId(string userId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("LikeService");
            var response = await client.GetAsync($"/api/likes/user/{userId}");

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

    public async Task<bool> HasUserLikedPost(string userId, Guid postId)
    {
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