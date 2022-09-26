using oed_feedpoller.Models;

namespace oed_feedpoller.Interfaces;
public interface ICursorService
{
    /// <summary>
    /// Gets a cursor by name. The cursor is a global asset and its access is exclusive. 
    /// </summary>
    /// <param name="cursorName">The name of the cursor</param>
    /// <returns>A cursor with an ID that can be used as a parameter for accessing a feed at a certain location</returns>
    Task<Cursor> GetCursor(string cursorName);

    /// <summary>
    /// Updates the cursor by name. The cursor is a global asset and its access is exclusive. 
    /// </summary>
    /// <param name="cursor">The cursor containing the name and updated value</param>
    /// <returns>Nothing, but throws if the updating fails for some reason</returns>
    Task UpdateCursor(Cursor cursor);
}
