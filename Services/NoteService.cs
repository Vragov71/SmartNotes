using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication1.Services
{
    public class NoteService : INoteService
    {
        private readonly AppDbContext _context;

        public NoteService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Note>> GetNotesAsync(string userId)
        {
            return await _context.Notes
                .Where(n => n.UserId == userId)
                .Include(n => n.Images)
                .Include(n => n.Files)
                .Include(n => n.Tags)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Note?> GetNoteAsync(int id, string userId)
        {
            return await _context.Notes
                .Include(n => n.Images)
                .Include(n => n.Files)
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        }

        public async Task CreateNoteAsync(Note note, List<IFormFile> imageFiles, List<IFormFile> noteFiles, string tags, string userId, string imagesPath, string filesPath)
        {
            note.UserId = userId;
            note.CreatedAt = DateTime.Now;
            note.Images = new List<NoteImage>();
            note.Files = new List<NoteFile>();
            note.Tags = new List<Tag>();

            if (!string.IsNullOrEmpty(tags))
            {
                var tagNames = tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Distinct();
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    note.Tags.Add(tag);
                }
            }

            if (imageFiles != null && imageFiles.Any())
            {
                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(imagesPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        note.Images.Add(new NoteImage { ImageUrl = fileName });
                    }
                }
            }

            if (noteFiles != null && noteFiles.Any())
            {
                foreach (var file in noteFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(filesPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        note.Files.Add(new NoteFile
                        {
                            FileName = fileName,
                            OriginalName = file.FileName,
                            ContentType = file.ContentType
                        });
                    }
                }
            }

            _context.Add(note);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateNoteAsync(int id, string title, string content, DateTime? createdAt, List<IFormFile> imageFiles, List<IFormFile> noteFiles, List<int> deleteImageIds, List<int> deleteFileIds, string tags, string userId, string imagesPath, string filesPath)
        {
            var existingNote = await _context.Notes
                .Include(n => n.Images)
                .Include(n => n.Files)
                .Include(n => n.Tags)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (existingNote == null)
            {
                return;
            }

            existingNote.Title = title;
            existingNote.Content = content;
            existingNote.CreatedAt = createdAt ?? existingNote.CreatedAt;

            existingNote.Tags.Clear();
            if (!string.IsNullOrEmpty(tags))
            {
                var tagNames = tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).Distinct();
                foreach (var tagName in tagNames)
                {
                    var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Name == tagName);
                    if (tag == null)
                    {
                        tag = new Tag { Name = tagName };
                        _context.Tags.Add(tag);
                    }
                    existingNote.Tags.Add(tag);
                }
            }

            if (deleteImageIds != null && deleteImageIds.Any())
            {
                var imagesToDelete = existingNote.Images.Where(img => deleteImageIds.Contains(img.Id)).ToList();
                foreach (var image in imagesToDelete)
                {
                    var filePath = Path.Combine(imagesPath, image.ImageUrl);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    _context.NoteImages.Remove(image);
                }
            }

            if (deleteFileIds != null && deleteFileIds.Any())
            {
                var filesToDelete = existingNote.Files.Where(f => deleteFileIds.Contains(f.Id)).ToList();
                foreach (var file in filesToDelete)
                {
                    var filePath = Path.Combine(filesPath, file.FileName);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    _context.NoteFiles.Remove(file);
                }
            }

            if (imageFiles != null && imageFiles.Any())
            {
                if (existingNote.Images == null)
                    existingNote.Images = new List<NoteImage>();

                foreach (var imageFile in imageFiles)
                {
                    if (imageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(imagesPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        existingNote.Images.Add(new NoteImage { ImageUrl = fileName });
                    }
                }
            }

            if (noteFiles != null && noteFiles.Any())
            {
                if (existingNote.Files == null)
                    existingNote.Files = new List<NoteFile>();

                foreach (var file in noteFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(filesPath, fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        existingNote.Files.Add(new NoteFile
                        {
                            FileName = fileName,
                            OriginalName = file.FileName,
                            ContentType = file.ContentType
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteNoteAsync(int id, string userId, string imagesPath, string filesPath)
        {
            var note = await _context.Notes
                .Include(n => n.Images)
                .Include(n => n.Files)
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (note != null)
            {
                if (note.Images != null)
                {
                    foreach (var image in note.Images)
                    {
                        var filePath = Path.Combine(imagesPath, image.ImageUrl);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                if (note.Files != null)
                {
                    foreach (var file in note.Files)
                    {
                        var filePath = Path.Combine(filesPath, file.FileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteImageAsync(int noteId, int imageId, string userId, string imagesPath)
        {
            var image = await _context.NoteImages
                .Include(i => i.Note)
                .FirstOrDefaultAsync(i => i.Id == imageId && i.NoteId == noteId && i.Note.UserId == userId);

            if (image != null)
            {
                var filePath = Path.Combine(imagesPath, image.ImageUrl);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.NoteImages.Remove(image);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteFileAsync(int noteId, int fileId, string userId, string filesPath)
        {
            var file = await _context.NoteFiles
                .Include(f => f.Note)
                .FirstOrDefaultAsync(f => f.Id == fileId && f.NoteId == noteId && f.Note.UserId == userId);

            if (file != null)
            {
                var filePath = Path.Combine(filesPath, file.FileName);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
                _context.NoteFiles.Remove(file);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<NoteFile?> GetFileAsync(string fileName, string userId)
        {
            return await _context.NoteFiles
                .Include(f => f.Note)
                .FirstOrDefaultAsync(f => f.FileName == fileName && f.Note.UserId == userId);
        }
    }
}