using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Note
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Заглавието е задължително")]
    [StringLength(100, ErrorMessage = "Заглавието не може да бъде по-дълго от 100 символа")]
    [Display(Name = "Заглавие")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Съдържанието е задължително")]
    [Display(Name = "Съдържание")]
    public string Content { get; set; } = string.Empty;

    [Display(Name = "Дата на създаване")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = string.Empty;

    public ICollection<NoteImage> Images { get; set; } = new List<NoteImage>();
    public ICollection<NoteFile> Files { get; set; } = new List<NoteFile>();
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}