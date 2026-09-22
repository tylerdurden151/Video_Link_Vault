using Backend_Link_Vault.Data;
using Backend_Link_Vault.Interfaces;
using Backend_Link_Vault.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend_Link_Vault.Services;
// This class implements the IVideoLinkRepository interface and provides methods for managing video links in the database using
// Entity Framework Core.
public class EfVideoLinkRepository : IVideoLinkRepository
{
    private readonly AppDbContext _context;

    public EfVideoLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<VideoLink>> GetForUserAsync(Guid userId) =>
        _context.VideoLinks
            .Where(v => v.UserId == userId)
            .ToListAsync();

    public async Task<VideoLink> AddAsync(VideoLink link)
    {
        _context.VideoLinks.Add(link);
        await _context.SaveChangesAsync();
        return link;
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid linkId)
    {
        var link = await _context.VideoLinks
            .FirstOrDefaultAsync(v => v.Id == linkId && v.UserId == userId);

        if (link is null) return false;

        _context.VideoLinks.Remove(link);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task SeedDemoDataAsync(Guid userId)
    {
        DateTime DaysAgo(int n) => DateTime.UtcNow.AddDays(-n);

        var seed = new List<VideoLink>
        {
            new() { UserId = userId, Url = "https://youtube.com/shorts/IuOt6euql7s", Platform = Platform.YouTube, Title = "Lovestruck", ThumbnailUrl = "https://img.youtube.com/vi/IuOt6euql7s/hqdefault.jpg", Category = "kennygifs", Tags = new() { "funny" }, CreatedAtUtc = DaysAgo(3) },
            // ...same seed list you already have in VideoLinkStore, unchanged
        };

        foreach (var link in seed)
        {
            link.Id = Guid.NewGuid();
        }

        _context.VideoLinks.AddRange(seed);
        await _context.SaveChangesAsync();
    }
}
