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
    [MaxLength(25)]
    public string UserName { get; set; } = null!;

    [Required]
    public bool Enabled { get; set; } = true;

    [Required]
    public bool EnabledWhisperLog { get; set; } = true;

    [Required]
    public bool Known { get; set; } = false;

    [Required]
    public bool Verified { get; set; } = false;

    [Required]
    [MaxLength(30)]
    public string AccessToken { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string RefreshToken { get; set; } = null!;

    public int? SupinicApiUser { get; set; }

    [MaxLength(65)]
    public string? SupinicApiKey { get; set; } = null!;

    public virtual List<Connection> Connections { get; set; } = null!;

    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bot>(entity =>
        {
            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.EnabledWhisperLog).HasDefaultValue(true);
            entity.Property(e => e.Known).HasDefaultValue(false);
            entity.Property(e => e.Verified).HasDefaultValue(false);
        });
    }
}
