
using Black_order.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;


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


            // Configuration for the Product entity
            modelBuilder.Entity<Product>(x =>
            {

                // Configure the Id property to be generated on add
                x.Property(x => x.Id).ValueGeneratedOnAdd();


                // Configure the relationship between Product and User
                x.HasOne(x => x.User)
                  .WithMany()
                  .HasForeignKey(x => x.UserId);


                // Configure the Name property to be required and have a maximum length of 25 characters
                x.Property(x => x.Name).IsRequired()
                .HasMaxLength(25);


                // Configure the Description property to have a maximum length of 100 characters
                x.Property(x => x.Description).HasMaxLength(100);


                // Configure the Price property to have a decimal type with precision and scale
                x.Property(x => x.Price).HasColumnType("decimal(18,2)").IsRequired();



                //Relationship with Category
                x.HasOne(x => x.Category)
                 .WithMany(x => x.Products)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.SetNull);


                // Configure the Available property to be required
                x.Property(x => x.Available).IsRequired();

                // Configure the Approved property to be required
                x.Property(x => x.Approved).IsRequired();

                




            });

            // Configuration for the Category entity
            modelBuilder.Entity<Category>(x =>
            {
                // Configure the Id property to be generated on add
                x.Property(x => x.Id).ValueGeneratedOnAdd();

                // Configure the Name property to be required and have a maximum length of 25 characters
                x.Property(x => x.Name).IsRequired()
                .HasMaxLength(25);

                // Configure the Description property to be required and have a maximum length of 500 characters
                x.Property(x => x.Description).IsRequired()
                .HasMaxLength(500);

            });


            // Configuration for the ProductImage entity
            modelBuilder.Entity<ProductImage>(x =>
            {
                // Configure the Id property to be generated on add
                x.Property(x => x.Id).ValueGeneratedOnAdd();
                // Configure the Url property to be required
                x.Property(x => x.Url).IsRequired();
                // Configure the relationship between ProductImage and Product
                x.HasOne(x => x.Product)
                 .WithMany(x => x.Images)
                 .HasForeignKey(x => x.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<CategoryImage>(x =>
            {
                // Configure the Url property to be required
                x.Property(x => x.Url).IsRequired();
                // Configure the relationship between CategoryImage and Category
                x.HasOne(x => x.Category)
                 .WithMany(x => x.Images)
                 .HasForeignKey(x => x.CategoryId)
                 .OnDelete(DeleteBehavior.Cascade);
            });




        }

        // Define DbSet properties
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

        public DbSet<CategoryImage> CategoryImage { get; set; }


    }
}
