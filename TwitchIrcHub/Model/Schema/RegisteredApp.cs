using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace TwitchIrcHub.Model.Schema;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class RegisteredApp
{
    [Key]
    [Required]
    public int RegisteredAppId { get; set; }

    [Required]
    [MaxLength(255)]
    public string AppName { get; set; } = null!;

    [Required]
    [MinLength(32)]
    [MaxLength(32)]
    public string Key { get; set; } = null!;

    [Required]
    [Column(TypeName = "TIMESTAMP")]
    public DateTime AddDate { get; set; }

    public virtual List<Connection> Connections { get; set; } = null!;
    
    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RegisteredApp>(entity =>
        {
            entity.Property(e => e.RegisteredAppId).ValueGeneratedOnAdd();
            entity.Property(e => e.AddDate).ValueGeneratedOnAdd();
        });
    }
}
