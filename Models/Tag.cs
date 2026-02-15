using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Tag
{
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Note> Notes { get; set; } = new List<Note>();
}