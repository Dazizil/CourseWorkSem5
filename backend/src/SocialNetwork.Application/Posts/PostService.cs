using Microsoft.EntityFrameworkCore;
using SocialNetwork.Application.Common.Exceptions;
using SocialNetwork.Application.Common.Interfaces;
using SocialNetwork.Application.Common.Models;
using SocialNetwork.Domain.Constants;
using SocialNetwork.Domain.Entities;

namespace SocialNetwork.Application.Posts;

public class PostService(IApplicationDbContext db, ICurrentUserService currentUser) : IPostService
{
    public async Task<PagedResult<PostDto>> GetFeedAsync(Guid? authorId, int page, int pageSize, CancellationToken ct = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var posts = db.Posts.AsNoTracking();
        if (authorId is not null)
            posts = posts.Where(p => p.AuthorId == authorId);

        var total = await posts.CountAsync(ct);
        var items = await posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(p => new PostDto(p.Id, p.Content, p.AuthorId, p.Author.UserName, p.Author.DisplayName, p.CreatedAt))
            .ToListAsync(ct);

        return new PagedResult<PostDto>(items, page, pageSize, total);
    }

    public async Task<PostDto> CreateAsync(CreatePostRequest request, CancellationToken ct = default)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Требуется авторизация.");
        var author = await db.Users.SingleAsync(u => u.Id == userId, ct);

        var post = new Post { Content = request.Content.Trim(), AuthorId = userId };
        db.Posts.Add(post);
        await db.SaveChangesAsync(ct);

        return new PostDto(post.Id, post.Content, author.Id, author.UserName, author.DisplayName, post.CreatedAt);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var post = await db.Posts.SingleOrDefaultAsync(p => p.Id == id, ct)
                   ?? throw new NotFoundException("Пост не найден.");

        var canModerate = currentUser.IsInRole(RoleNames.Moderator) || currentUser.IsInRole(RoleNames.Admin);
        if (post.AuthorId != currentUser.UserId && !canModerate)
            throw new ForbiddenException("Удалять можно только свои посты.");

        db.Posts.Remove(post);
        await db.SaveChangesAsync(ct);
    }
}
