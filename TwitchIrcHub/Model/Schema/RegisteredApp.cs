using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace TwitchIrcHub.Model.Schema;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RegisteredApp
{
    [Key]
    [Required]
    public int Id { get; set; }

    [Required]
    public string AppName { get; set; } = null!;

    [Required]
    [MinLength(32)]
    [MaxLength(32)]
    public string Key { get; set; } = null!;

    [Required]
    [Column(TypeName = "TIMESTAMP")]
    public DateTime AddDate { get; set; }

    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredApp>(entity =>
        {
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.AddDate).ValueGeneratedOnAdd();
        });
    }
}
