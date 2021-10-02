using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace TwitchIrcHub.Model.Schema;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Bot
{
    [Key]
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public string UserName { get; set; } = null!;

    [Required]
    public bool Enabled { get; set; } = true;

    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bot>(entity =>
        {
            entity.Property(e => e.Enabled).HasDefaultValue(true);
        });
    }
}
