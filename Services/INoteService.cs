using WebApplication1.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Services
{
    public interface INoteService
    {
        Task<List<Note>> GetNotesAsync(string userId);
        Task<Note?> GetNoteAsync(int id, string userId);
        Task CreateNoteAsync(Note note, List<IFormFile> imageFiles, List<IFormFile> noteFiles, string tags, string userId, string imagesPath, string filesPath);
        Task UpdateNoteAsync(int id, string title, string content, DateTime? createdAt, List<IFormFile> imageFiles, List<IFormFile> noteFiles, List<int> deleteImageIds, List<int> deleteFileIds, string tags, string userId, string imagesPath, string filesPath);
        Task DeleteNoteAsync(int id, string userId, string imagesPath, string filesPath);
        Task DeleteImageAsync(int noteId, int imageId, string userId, string imagesPath);
        Task DeleteFileAsync(int noteId, int fileId, string userId, string filesPath);
        Task<NoteFile?> GetFileAsync(string fileName, string userId);
    }
}