using Microsoft.EntityFrameworkCore;

namespace EFCoreDemos.Database;

public class DemoDbContext : DbContext
{
   public DbSet<Product> Products { get; set; }
   public DbSet<Studio> Studios { get; set; }
   public DbSet<Price> Prices { get; set; }
   public DbSet<Seller_Product> SellerProducts { get; set; }
   public DbSet<Seller> Sellers { get; set; }

   public DemoDbContext(DbContextOptions<DemoDbContext> options)
      : base(options)
   {
   }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      base.OnModelCreating(modelBuilder);

      var studios = GenerateStudio(5);
      var products = GenerateProducts(studios, 100);
      var prices = GeneratePrices(products, 10);
      var sellers = GenerateSellers(2);
      var sellerProducts = GenerateSellerProducts(products, sellers);

      modelBuilder.Entity<Studio>(builder =>
                                  {
                                     builder.Property(s => s.Id).ValueGeneratedNever();
                                     builder.HasData(studios);
                                  });
      modelBuilder.Entity<Product>(builder =>
                                   {
                                      builder.Property(p => p.Id).ValueGeneratedNever();
                                      builder.HasData(products);
                                   });
      modelBuilder.Entity<Price>(builder =>
                                 {
                                    builder.Property(p => p.Id).ValueGeneratedNever();
                                    builder.Property(p => p.Value).HasPrecision(19, 2);
                                    builder.HasData(prices);
                                 });
      modelBuilder.Entity<Seller>(builder =>
                                  {
                                     builder.Property(s => s.Id).ValueGeneratedNever();

                                     builder.HasMany(s => s.Products)
                                            .WithMany(p => p.Sellers)
                                            .UsingEntity<Seller_Product>(sellerProductsBuilder => sellerProductsBuilder.HasOne(sp => sp.Product).WithMany(),
                                                                         sellerProductsBuilder => sellerProductsBuilder.HasOne(sp => sp.Seller).WithMany(),
                                                                         sellerProductsBuilder =>
                                                                         {
                                                                            sellerProductsBuilder.ToTable("SellerProducts");
                                                                            sellerProductsBuilder.HasKey(sp => new { sp.ProductId, sp.SellerId });
                                                                            sellerProductsBuilder.HasData(sellerProducts);
                                                                         });
                                     builder.HasData(sellers);
                                  });
   }

   private static List<Price> GeneratePrices(
      List<Product> products,
      int numberOfPricesPerProduct)
   {
      var prices = new List<Price>();

      for (var i = 0; i < products.Count; i++)
      {
         var product = products[i];
         prices.AddRange(Enumerable.Range(1, numberOfPricesPerProduct)
                                   .Select(j => new Price
                                                {
                                                   Id = i * numberOfPricesPerProduct + j,
                                                   ProductId = product.Id,
                                                   Value = 42
                                                }));
      }

      return prices;
   }

   private static List<Seller> GenerateSellers(int numberOfSellers)
   {
      return Enumerable.Range(1, numberOfSellers)
                       .Select(i => new Seller
                                    {
                                       Id = i,
                                       Name = $"Seller {i}"
                                    })
                       .ToList();
   }

   private static List<object> GenerateSellerProducts(
      List<Product> products,
      List<Seller> sellers)
   {
      var sellersProduct = new List<object>();

      foreach (var product in products)
      foreach (var seller in sellers)
      {
         sellersProduct.Add(new
                            {
                               ProductId = product.Id,
                               SellerId = seller.Id
                            });
      }

      return sellersProduct;
   }

   private static List<Studio> GenerateStudio(int numberOfStudios)
   {
      return Enumerable.Range(1, numberOfStudios)
                       .Select(i => new Studio
                                    {
                                       Id = i,
                                       Name = $"Studio {i}"
                                    })
                       .ToList();
   }

   private static List<Product> GenerateProducts(
      List<Studio> studios,
      int numberOfProducts)
   {
      return Enumerable.Range(1, numberOfProducts)
                       .Select(i => new Product
                                    {
                                       Id = i,
                                       Name = i % 2 == 0 ? "Infinity" : "Endgame",
                                       DeliverableFrom = new DateTime(2000, 01, 01),
                                       DeliverableUntil = new DateTime(2030, 01, 01),
                                       StudioId = studios[i % studios.Count].Id
                                    })
                       .ToList();
   }
}
