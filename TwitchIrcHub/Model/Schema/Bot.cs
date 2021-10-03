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

    [Required]
    public bool EnabledWhisperLog { get; set; } = true;

    [Required]
    public string ClientId { get; set; } = null!;

    [Required]
    public string ClientSecret { get; set; } = null!;

    [Required]
    public string AccessToken { get; set; } = null!;

    [Required]
    public string RefreshToken { get; set; } = null!;

    [Required]
    public string SupinicApiUser { get; set; } = null!;

    [Required]
    public string SupinicApiKey { get; set; } = null!;

    public virtual List<Connection> Connections { get; set; } = null!;

    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Bot>(entity =>
        {
            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.EnabledWhisperLog).HasDefaultValue(true);
        });
    }
}
