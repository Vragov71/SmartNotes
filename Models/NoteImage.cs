namespace WebApplication1.Models;

public class NoteImage
{
    public int Id { get; set; }
    public string ImageUrl { get; set; }
    public int NoteId { get; set; }
    public Note Note { get; set; }
} 