﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace TwitchIrcHub.Model.Schema;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
public class Channel
{
    [Key]
    [Required]
    public int RoomId { get; set; }

    [Required]
    public string ChannelName { get; set; } = null!;

    [Required]
    public bool Enabled { get; set; } = true;

    [Required]
    public int MaxIrcMessageLength { get; set; } = 450;

    [Required]
    public int MinCooldown { get; set; } = 0;

    protected internal static void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Channel>(entity =>
        {
            entity.Property(e => e.Enabled).HasDefaultValue(true);
            entity.Property(e => e.MaxIrcMessageLength).HasDefaultValue(450);
            entity.Property(e => e.MinCooldown).HasDefaultValue(0);
        });
    }
}
