using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace WebApplication1.Tests
{
    [TestFixture]
    public class NoteServiceTests
    {
        private AppDbContext _context;
        private NoteService _service;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) 
                .Options;

            _context = new AppDbContext(options);
            _service = new NoteService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task CreateNoteAsync_ShouldAddNoteToDatabase()
        {
            var note = new Note { Title = "Test Note", Content = "Test Content" };
            var userId = "user123";
            var imageFiles = new List<IFormFile>();
            var noteFiles = new List<IFormFile>();
            var tags = "tag1, tag2";

            await _service.CreateNoteAsync(note, imageFiles, noteFiles, tags, userId, "imagesPath", "filesPath");

            var savedNote = await _context.Notes.Include(n => n.Tags).FirstOrDefaultAsync();
            Assert.That(savedNote, Is.Not.Null);
            Assert.That(savedNote.Title, Is.EqualTo("Test Note"));
            Assert.That(savedNote.UserId, Is.EqualTo(userId));
            Assert.That(savedNote.Tags.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetNotesAsync_ShouldReturnOnlyUserNotes()
        {
            var userId1 = "user1";
            var userId2 = "user2";

            _context.Notes.Add(new Note { Title = "Note 1", UserId = userId1, Content = "Content 1" });
            _context.Notes.Add(new Note { Title = "Note 2", UserId = userId2, Content = "Content 2" });
            await _context.SaveChangesAsync();

            var notes = await _service.GetNotesAsync(userId1);

            Assert.That(notes.Count, Is.EqualTo(1));
            Assert.That(notes[0].Title, Is.EqualTo("Note 1"));
        }

        [Test]
        public async Task UpdateNoteAsync_ShouldUpdateNoteDetails()
        {
            var userId = "user1";
            var note = new Note { Title = "Old Title", Content = "Old Content", UserId = userId };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            await _service.UpdateNoteAsync(note.Id, "New Title", "New Content", null, new List<IFormFile>(), new List<IFormFile>(), new List<int>(), new List<int>(), "newTag", userId, "imagesPath", "filesPath");

            var updatedNote = await _context.Notes.Include(n => n.Tags).FirstOrDefaultAsync(n => n.Id == note.Id);
            Assert.That(updatedNote.Title, Is.EqualTo("New Title"));
            Assert.That(updatedNote.Content, Is.EqualTo("New Content"));
            Assert.That(updatedNote.Tags.Count, Is.EqualTo(1));
            Assert.That(updatedNote.Tags.First().Name, Is.EqualTo("newTag"));
        }

        [Test]
        public async Task DeleteNoteAsync_ShouldRemoveNoteFromDatabase()
        {
            var userId = "user1";
            var note = new Note { Title = "To Delete", UserId = userId };
            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            await _service.DeleteNoteAsync(note.Id, userId, "imagesPath", "filesPath");

            var deletedNote = await _context.Notes.FindAsync(note.Id);
            Assert.That(deletedNote, Is.Null);
        }
    }
}