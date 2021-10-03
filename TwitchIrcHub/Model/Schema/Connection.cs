using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace TwitchIrcHub.Model.Schema;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Connection
{
    [ForeignKey("Bot")]
    [Required]
    public int BotUserId { get; set; }

    public virtual Bot Bot { get; set; } = null!;
    
    [ForeignKey("Channel")]
    [Required]
    public int RoomId { get; set; }
    
    public virtual Channel Channel { get; set; } = null!;

    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Connection>(entity =>
        {
            entity.HasKey(nameof(BotUserId), nameof(RoomId));
        });
    }
}
