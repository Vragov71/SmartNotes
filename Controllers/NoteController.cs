using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using WebApplication1.Services;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class NoteController : Controller
    {
        private readonly INoteService _noteService;
        private readonly string _imagesPath;
        private readonly string _filesPath;

        public NoteController(INoteService noteService)
        {
            _noteService = noteService;
            _imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            _filesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/files");
            
            if (!Directory.Exists(_imagesPath))
            {
                Directory.CreateDirectory(_imagesPath);
            }
            if (!Directory.Exists(_filesPath))
            {
                Directory.CreateDirectory(_filesPath);
            }
        }
        
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notes = await _noteService.GetNotesAsync(userId);
            return View(notes);
        }

        public IActionResult Create()
        {
            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Note note, List<IFormFile> imageFiles, List<IFormFile> noteFiles, string tags)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _noteService.CreateNoteAsync(note, imageFiles, noteFiles, tags, userId, _imagesPath, _filesPath);
                return RedirectToAction(nameof(Index));
            }
            return View(note);
        }
        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _noteService.GetNoteAsync(id.Value, userId);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _noteService.DeleteNoteAsync(id, userId, _imagesPath, _filesPath);
            return RedirectToAction(nameof(Index));
        }
        
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _noteService.GetNoteAsync(id, userId);
            if (note == null)
            {
                return NotFound();  
            }
            return View(note);  
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int Id, string Title, string Content, DateTime? CreatedAt, List<IFormFile> imageFiles, List<IFormFile> noteFiles, List<int> deleteImageIds, List<int> deleteFileIds, string tags)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _noteService.UpdateNoteAsync(Id, Title, Content, CreatedAt, imageFiles, noteFiles, deleteImageIds, deleteFileIds, tags, userId, _imagesPath, _filesPath);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Note/DeleteImage/{noteId}/{imageId}")]
        public async Task<IActionResult> DeleteImage(int noteId, int imageId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _noteService.DeleteImageAsync(noteId, imageId, userId, _imagesPath);
            return RedirectToAction(nameof(Edit), new { id = noteId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Note/DeleteFile/{noteId}/{fileId}")]
        public async Task<IActionResult> DeleteFile(int noteId, int fileId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _noteService.DeleteFileAsync(noteId, fileId, userId, _filesPath);
            return RedirectToAction(nameof(Edit), new { id = noteId });
        }

        public async Task<IActionResult> DownloadFile(string fileName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var file = await _noteService.GetFileAsync(fileName, userId);
                
            if (file == null)
                return NotFound();
            var filePath = Path.Combine(_filesPath, file.FileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound();
            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;
            return File(memory, file.ContentType, file.OriginalName);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var note = await _noteService.GetNoteAsync(id, userId);
            if (note == null)
            {
                return NotFound();
            }
            return View(note);
        }
    }
}