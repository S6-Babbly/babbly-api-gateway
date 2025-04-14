using babbly_api_gateway.Models;
using babbly_api_gateway.Services;

namespace babbly_api_gateway.Aggregators;

public class ProfileAggregator
{
    private readonly IUserService _userService;
    private readonly IPostService _postService;
    private readonly ICommentService _commentService;

    public ProfileAggregator(IUserService userService, IPostService postService, ICommentService commentService)
    {
        _userService = userService;
        _postService = postService;
        _commentService = commentService;
    }

    public async Task<UserProfile?> GetUserProfileById(Guid userId, int postsPage = 1, int postsPageSize = 10)
    {
        var user = await _userService.GetUserById(userId);
        if (user == null)
            return null;

        var posts = await _postService.GetPostsByUserId(userId, postsPage, postsPageSize);
        var comments = await _commentService.GetCommentsByUserId(userId, 1, 10);
        var followers = await _userService.GetFollowers(userId);
        var following = await _userService.GetFollowing(userId);

        return new UserProfile
        {
            User = user,
            Posts = posts,
            Comments = comments,
            PostsCount = posts.Count,
            CommentsCount = comments.Count,
            FollowersCount = followers.Count,
            FollowingCount = following.Count
        };
    }

    public async Task<UserProfile?> GetUserProfileByUsername(string username, int postsPage = 1, int postsPageSize = 10)
    {
        var user = await _userService.GetUserByUsername(username);
        if (user == null)
            return null;

        return await GetUserProfileById(user.Id, postsPage, postsPageSize);
    }
} 