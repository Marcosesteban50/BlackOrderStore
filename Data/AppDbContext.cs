
using Black_order.DTOs.UserDTOs;
using Black_order.Models;
using Black_order.Models.Guides;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Cart;
using Models.Favorite;


namespace Black_order.Data
{
    public class AppDbContext : IdentityDbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> opc) : base(opc)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureProduct(modelBuilder.Entity<Product>());
            ConfigureCategory(modelBuilder.Entity<Category>());
            ConfigureProductImage(modelBuilder.Entity<ProductImage>());
            ConfigureCategoryImage(modelBuilder.Entity<CategoryImage>());
            ConfigureUserProfile(modelBuilder.Entity<UserProfile>());
            ConfigureFavorite(modelBuilder.Entity<Favorite>());
            ConfigureFavoriteItem(modelBuilder.Entity<FavoriteItems>());
            ConfigureGuideCategory(modelBuilder.Entity<GuideCategory>());
            ConfigureGuide(modelBuilder.Entity<Guide>());
            ConfigureGuideMedia(modelBuilder.Entity<GuideMedia>());
        }

        private static void ConfigureProduct(EntityTypeBuilder<Product> entity)
        {
            entity.Property(product => product.Id).ValueGeneratedOnAdd();
            entity.Property(product => product.Name).IsRequired().HasMaxLength(200);
            entity.Property(product => product.Description).HasMaxLength(100);
            entity.Property(product => product.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(product => product.Available).IsRequired();
            entity.Property(product => product.Approved).IsRequired();

            entity.HasOne(product => product.User)
                .WithMany()
                .HasForeignKey(product => product.UserId);

            entity.HasOne(product => product.Category)
                .WithMany(category => category.Products)
                .HasForeignKey(product => product.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        private static void ConfigureCategory(EntityTypeBuilder<Category> entity)
        {
            entity.Property(category => category.Id).ValueGeneratedOnAdd();
            entity.Property(category => category.Name).IsRequired().HasMaxLength(25);
            entity.Property(category => category.Description).IsRequired().HasMaxLength(500);
        }

        private static void ConfigureProductImage(EntityTypeBuilder<ProductImage> entity)
        {
            entity.ToTable("ProductImage");
            entity.Property(image => image.Id).ValueGeneratedOnAdd();
            entity.Property(image => image.Url).IsRequired();
            entity.HasOne(image => image.Product)
                .WithMany(product => product.Images)
                .HasForeignKey(image => image.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureFavorite(EntityTypeBuilder<Favorite> entity)
        {
            entity.HasOne(favorite => favorite.User)
                .WithMany()
                .HasForeignKey(favorite => favorite.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureFavoriteItem(EntityTypeBuilder<FavoriteItems> entity)
        {
            entity.HasOne(item => item.Favorite)
                .WithMany(favorite => favorite.Items)
                .HasForeignKey(item => item.FavoriteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(item => item.Product)
                .WithMany()
                .HasForeignKey(item => item.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureCategoryImage(EntityTypeBuilder<CategoryImage> entity)
        {
            entity.ToTable("CategoryImage");
            entity.Property(image => image.Url).IsRequired();
            entity.HasOne(image => image.Category)
                .WithMany(category => category.Images)
                .HasForeignKey(image => image.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureUserProfile(EntityTypeBuilder<UserProfile> entity)
        {
            entity.Property(profile => profile.UserId).IsRequired();
            entity.HasOne(profile => profile.User)
                .WithMany()
                .HasForeignKey(profile => profile.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        private static void ConfigureGuideCategory(EntityTypeBuilder<GuideCategory> entity)
        {
            entity.ToTable("GuideCategories");
            entity.Property(category => category.Name).IsRequired().HasMaxLength(100);
            entity.Property(category => category.Slug).IsRequired().HasMaxLength(120);
            entity.Property(category => category.Description).HasMaxLength(500);
            entity.HasIndex(category => category.Slug).IsUnique();
        }

        private static void ConfigureGuide(EntityTypeBuilder<Guide> entity)
        {
            entity.ToTable("Guides");
            entity.Property(guide => guide.Title).IsRequired().HasMaxLength(200);
            entity.Property(guide => guide.Slug).IsRequired().HasMaxLength(200);
            entity.Property(guide => guide.Summary).HasMaxLength(500);
            entity.Property(guide => guide.ContentHtml).IsRequired();
            entity.Property(guide => guide.CoverUrl).HasMaxLength(500);
            entity.Property(guide => guide.Level).HasMaxLength(50);
            entity.Property(guide => guide.Expansion).HasMaxLength(50);

            entity.HasIndex(guide => guide.Slug).IsUnique();
            entity.HasIndex(guide => new { guide.IsPublished, guide.PublishedAt });

            entity.HasOne(guide => guide.Category)
                .WithMany(category => category.Guides)
                .HasForeignKey(guide => guide.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(guide => guide.Author)
                .WithMany()
                .HasForeignKey(guide => guide.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private static void ConfigureGuideMedia(EntityTypeBuilder<GuideMedia> entity)
        {
            entity.ToTable("GuideMedia");
            entity.Property(media => media.Provider).IsRequired().HasMaxLength(50);
            entity.Property(media => media.Url).IsRequired().HasMaxLength(500);
            entity.Property(media => media.ExternalId).HasMaxLength(100);
            entity.Property(media => media.Caption).HasMaxLength(300);
            entity.Property(media => media.MediaType)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(20);

            entity.HasIndex(media => new { media.GuideId, media.SortOrder });

            entity.HasOne(media => media.Guide)
                .WithMany(guide => guide.Media)
                .HasForeignKey(media => media.GuideId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<CategoryImage> CategoryImage => Set<CategoryImage>();
        public DbSet<Cart> Cart => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<FavoriteItems> FavoriteItems => Set<FavoriteItems>();
        public DbSet<Guide> Guides => Set<Guide>();
        public DbSet<GuideCategory> GuideCategories => Set<GuideCategory>();
        public DbSet<GuideMedia> GuideMediaItems => Set<GuideMedia>();

    }
}
