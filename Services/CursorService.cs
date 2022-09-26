using oed_feedpoller.Interfaces;
using oed_feedpoller.Models;

namespace oed_feedpoller.Services;

/// <summary>
/// Uses a storage client to access the function apps storage account containing the state for the cursor
/// </summary>
public class CursorService : ICursorService
{
    /// <inheritdoc/>
    public async Task<Cursor> GetCursor(string cursorName)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public async Task UpdateCursor(Cursor cursor)
    {
        throw new NotImplementedException();
    }
}
