using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Inventory_Management_System.Models;

namespace Inventory_Management_System.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryTag> InventoryTags { get; set; }
    public DbSet<InventoryAccess> InventoryAccesses { get; set; }
    public DbSet<CustomField> CustomFields { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<ItemLike> ItemLikes { get; set; }
    public DbSet<Discussion> Discussions { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Inventory>()
            .HasKey(i => i.Id);
        builder.Entity<Inventory>()
            .HasIndex(i => i.OwnerId);
        builder.Entity<Inventory>()
            .HasIndex(i => i.CreatedAt);
        builder.Entity<Inventory>()
            .Property(i => i.Version)
            .IsConcurrencyToken();

        builder.Entity<Inventory>()
            .HasMany(i => i.Items)
            .WithOne(it => it.Inventory)
            .HasForeignKey(it => it.InventoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Inventory>()
            .HasMany(i => i.AccessControls)
            .WithOne(ac => ac.Inventory)
            .HasForeignKey(ac => ac.InventoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Inventory>()
            .HasMany(i => i.CustomFields)
            .WithOne(cf => cf.Inventory)
            .HasForeignKey(cf => cf.InventoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Inventory>()
            .HasMany(i => i.Tags)
            .WithOne(t => t.Inventory)
            .HasForeignKey(t => t.InventoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<Inventory>()
            .HasMany(i => i.Discussions)
            .WithOne(d => d.Inventory)
            .HasForeignKey(d => d.InventoryId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<InventoryTag>()
            .HasKey(it => it.Id);
        builder.Entity<InventoryTag>()
            .HasIndex(it => new { it.InventoryId, it.Tag });

        builder.Entity<InventoryAccess>()
            .HasKey(ia => ia.Id);
        builder.Entity<InventoryAccess>()
            .HasIndex(ia => new { ia.InventoryId, ia.UserId })
            .IsUnique();

        builder.Entity<CustomField>()
            .HasKey(cf => cf.Id);
        builder.Entity<CustomField>()
            .HasIndex(cf => new { cf.InventoryId, cf.DisplayOrder });

        builder.Entity<Item>()
            .HasKey(i => i.Id);
        builder.Entity<Item>()
            .HasIndex(i => new { i.InventoryId, i.CustomId })
            .IsUnique();
        builder.Entity<Item>()
            .HasIndex(i => i.CreatedById);
        builder.Entity<Item>()
            .Property(i => i.Version)
            .IsConcurrencyToken();

        builder.Entity<Item>()
            .Property(i => i.CustomNumber1Value)
            .HasPrecision(18, 4);
        builder.Entity<Item>()
            .Property(i => i.CustomNumber2Value)
            .HasPrecision(18, 4);
        builder.Entity<Item>()
            .Property(i => i.CustomNumber3Value)
            .HasPrecision(18, 4);

        builder.Entity<Item>()
            .HasMany(i => i.Likes)
            .WithOne(il => il.Item)
            .HasForeignKey(il => il.ItemId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Entity<ItemLike>()
            .HasKey(il => il.Id);
        builder.Entity<ItemLike>()
            .HasIndex(il => new { il.ItemId, il.UserId })
            .IsUnique();

        builder.Entity<Discussion>()
            .HasKey(d => d.Id);
        builder.Entity<Discussion>()
            .HasIndex(d => d.InventoryId);
        builder.Entity<Discussion>()
            .HasIndex(d => d.UserId);
        builder.Entity<Discussion>()
            .HasIndex(d => new { d.InventoryId, d.CreatedAt });

        builder.Entity<UserPreference>()
            .HasKey(up => up.UserId);
        builder.Entity<UserPreference>()
            .HasOne(up => up.User)
            .WithOne()
            .HasForeignKey<UserPreference>(up => up.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

