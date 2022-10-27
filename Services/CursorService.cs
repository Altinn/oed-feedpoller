using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;

/// <summary>
/// Uses a storage client to access the function apps storage account containing the state for the cursor
/// </summary>
public class CursorService : ICursorService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CursorService> _logger;
    private const string CursorContainerName = "dacursor";
    private BlobServiceClient _blobServiceClient = null!;
    private BlobContainerClient _containerClient = null!;
    private bool _isConnected;

    public CursorService(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _configuration = configuration;
        _logger = loggerFactory.CreateLogger<CursorService>();
    }

    /// <inheritdoc/>
    public async Task<Cursor> GetCursor(string cursorName)
    {
        if (!_isConnected) await ConnectToBlobStorage();

        var blobClient = _containerClient.GetBlobClient(cursorName);
        if (await blobClient.ExistsAsync())
        {
            var downloadResult = await blobClient.DownloadContentAsync();
            try
            {

                var cursor = downloadResult.Value.Content.ToObjectFromJson<Cursor>();
                if (cursor == null)
                {
                    throw new NullReferenceException("Cursor deserialized to null");
                }

                _logger.LogInformation($"Cursor {cursorName} returned, pointing at {cursor.Value}");

                return cursor;
            }
            catch (Exception e)
            {
                _logger.LogError($"Unable to deserialize cursor {cursorName}, returning default cursor. Exception was: " + e.Message);
            }
        }
        else
        {
            _logger.LogInformation($"Cursor {cursorName} doesn't exist, returning default cursor");
        }

        return new Cursor
        {
            Name = cursorName,
            Value = null
        };
    }

    /// <inheritdoc/>
    public async Task UpdateCursor(Cursor cursor)
    {
        if (!_isConnected) await ConnectToBlobStorage();

        _logger.LogInformation($"Cursor {cursor.Name} updated, pointing at {cursor.Value}");

        var blobClient = _containerClient.GetBlobClient(cursor.Name);

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(cursor)));
        await blobClient.UploadAsync(ms, overwrite: true);
    }

    private async Task ConnectToBlobStorage()
    {
        _blobServiceClient = new BlobServiceClient(_configuration["AzureWebJobsStorage"]);
        _containerClient = _blobServiceClient.GetBlobContainerClient(CursorContainerName);
        if (!await _containerClient.ExistsAsync())
        {
            await _containerClient.CreateAsync();
        }

        _isConnected = true;
    }
}
