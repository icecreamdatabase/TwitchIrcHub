using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace TwitchIrcHub.Model.Schema
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public class BotData
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Key { get; set; } = null!;

        [Required]
        [MaxLength(255)]
        public string Value { get; set; } = null!;

        [Required]
        public DateTime LastUpdated { get; set; }

        protected internal static void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BotData>(entity =>
            {
                entity.HasIndex(e => e.Key).IsUnique();
                entity.Property(e => e.LastUpdated).ValueGeneratedOnAddOrUpdate();
            });
        }
    }
}
