using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using SocialGasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
            return BadRequest(new { message = "Invalid payload" });
        }

        try
        {
            if (payload.Households?.Any() == true)
            {
                var bulkOps = new List<WriteModel<Household>>();
                foreach (var h in payload.Households)
                {
                    h.SyncStatus = "Synced";
                    var filter = Builders<Household>.Filter.Eq(x => x.ClientGuid, h.ClientGuid);
                    bulkOps.Add(new ReplaceOneModel<Household>(filter, h) { IsUpsert = true });
                }
                await _households.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
            }

            if (payload.Citizens?.Any() == true)
            {
                var bulkOps = new List<WriteModel<Citizen>>();
                foreach (var c in payload.Citizens)
                {
                    c.SyncStatus = "Synced";
                    var filter = Builders<Citizen>.Filter.Eq(x => x.ClientGuid, c.ClientGuid);
                    bulkOps.Add(new ReplaceOneModel<Citizen>(filter, c) { IsUpsert = true });
                }
                await _citizens.BulkWriteAsync(bulkOps, new BulkWriteOptions { IsOrdered = false });
            }

            _logger.LogInformation("Synchronization completed successfully.");
            return Ok(new { success = true, message = "Synchronization completed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during synchronization");
            return StatusCode(500, new { message = "Server error", details = ex.Message });
        }
    }
}