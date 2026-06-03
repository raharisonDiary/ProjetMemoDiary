using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SocialGasy.Models;

[Route("api/[controller]")]
[ApiController]
public class SyncController : ControllerBase
{
    private readonly IMongoCollection<Household> _households;
    private readonly IMongoCollection<Citizen> _citizens;
    private readonly ILogger<SyncController> _logger;

    public SyncController(IMongoDatabase database, ILogger<SyncController> logger)
    {
        _households = database.GetCollection<Household>("Households");
        _citizens = database.GetCollection<Citizen>("Citizens");
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadData([FromBody] SyncPayload payload)
    {
        if (payload == null)
        {
            return BadRequest(new { message = "Payload tsy mety" });
        }

        try
        {
            // 1. Households Sync
            if (payload.Households?.Any() == true)
            {
                var bulkOps = new List<WriteModel<Household>>();
                foreach (var h in payload.Households)
                {
                    // Raha tsy misy ID, avelao i MongoDB hamorona (ObjectId vaovao)
                    var filter = Builders<Household>.Filter.Eq(x => x.Id, h.Id);
                    bulkOps.Add(new ReplaceOneModel<Household>(filter, h) { IsUpsert = true });
                }
                await _households.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
            }

            // 2. Citizens Sync
            if (payload.Citizens?.Any() == true)
            {
                var bulkOps = new List<WriteModel<Citizen>>();
                foreach (var c in payload.Citizens)
                {
                    var filter = Builders<Citizen>.Filter.Eq(x => x.Id, c.Id);
                    bulkOps.Add(new ReplaceOneModel<Citizen>(filter, c) { IsUpsert = true });
                }
                await _citizens.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
            }

            _logger.LogInformation("Sync vita soa aman-tsara.");
            return Ok(new { success = true, message = "Sync vita" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Nisy olana nandritra ny sync");
            return StatusCode(500, new { message = "Hadisoana teo amin'ny server", details = ex.Message });
        }
    }
}