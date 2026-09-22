using Microsoft.AspNetCore.Mvc;
using Backend_Link_Vault.DTO;
using Backend_Link_Vault.Models;
using Backend_Link_Vault.Services;
using Backend_Link_Vault.Interfaces;

namespace Backend_Link_Vault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VideoLinksController : ControllerBase
{
    private readonly IVideoLinkRepository _videoLinkRepository;
    private readonly UserStore _userStore;

    public VideoLinksController(IVideoLinkRepository videoLinkRepository, UserStore userStore)
    {
        _videoLinkRepository = videoLinkRepository;
        _userStore = userStore;
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<VideoLink>>> GetForUser(Guid userId)
    {
        if (!await UserExistsAsync(userId))
        {
            return NotFound("No account with that id.");
        }

        return Ok(await _videoLinkRepository.GetForUserAsync(userId));
    }

    [HttpPost("{userId}")]
    public async Task<ActionResult<VideoLink>> Create(Guid userId, CreateVideoLinkRequest request)
    {
        if (!await UserExistsAsync(userId))
        {
            return NotFound("No account with that id.");
        }

        var link = new VideoLink
        {
            UserId = userId,
            Url = request.Url,
            Platform = request.Platform!.Value,
            Title = request.Title,
            ThumbnailUrl = request.ThumbnailUrl,
            Category = request.Category,
            Tags = request.Tags,
            CreatedAtUtc = DateTime.UtcNow,
        };

        await _videoLinkRepository.AddAsync(link);

        return Ok(link);
    }

    [HttpDelete("{userId}/{linkId}")]
    public async Task<IActionResult> Delete(Guid userId, Guid linkId)
    {
        if (!await UserExistsAsync(userId))
        {
            return NotFound("No account with that id.");
        }

        var deleted = await _videoLinkRepository.DeleteAsync(userId, linkId);
        if (!deleted)
        {
            return NotFound("Link not found for this account.");
        }

        return NoContent();
    }

    private async Task<bool> UserExistsAsync(Guid userId) =>
        await _userStore.FindByIdAsync(userId) is not null;
}
