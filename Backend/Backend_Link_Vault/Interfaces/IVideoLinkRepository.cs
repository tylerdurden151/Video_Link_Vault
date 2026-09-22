using Backend_Link_Vault.Models;

namespace Backend_Link_Vault.Interfaces;

public interface IVideoLinkRepository
{
    Task<List<VideoLink>> GetForUserAsync(Guid userId);
    Task<VideoLink> AddAsync(VideoLink link);
    Task<bool> DeleteAsync(Guid userId, Guid linkId);
    Task SeedDemoDataAsync(Guid userId);
}
