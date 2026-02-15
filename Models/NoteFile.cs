namespace WebApplication1.Models;

public class NoteFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int NoteId { get; set; }
    public Note Note { get; set; }
} 