using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace EFCoreDemos.Database
{
   public class DemoDbContext : DbContext
   {
      public DbSet<Product> Products { get; set; }
      public DbSet<ProductGroup> ProductGroups { get; set; }
      public DbSet<Studio> Studios { get; set; }
      public DbSet<Price> Prices { get; set; }
      public DbSet<Seller> Sellers { get; set; }
      public DbSet<Seller_Product> SellerProducts { get; set; }

      public DemoDbContext(DbContextOptions<DemoDbContext> options)
         : base(options)
      {
      }

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);

         var groups = GenerateProductGroups(10);
         var studios = GenerateStudio(5);
         var products = GenerateProducts(groups, studios, 100);
         var prices = GeneratePrices(products, 10);
         var sellers = GenerateSellers(2);
         var sellerProducts = GenerateSellerProducts(products, sellers);

         modelBuilder.Entity<ProductGroup>().HasData(groups);
         modelBuilder.Entity<Studio>().HasData(studios);
         modelBuilder.Entity<Product>(builder =>
         {
            builder.HasIndex(p => new {p.ProductGroupId, p.Id})
                   .IncludeProperties(p => p.Name);
            builder.HasData(products);
         });
         modelBuilder.Entity<Price>().HasData(prices);
         modelBuilder.Entity<Seller>().HasData(sellers);
         modelBuilder.Entity<Seller_Product>(builder =>
         {
            builder.HasKey(e => new {e.ProductId, e.SellerId});
            builder.HasData(sellerProducts);
         });
      }

      private List<Price> GeneratePrices(
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

      private List<Seller> GenerateSellers(int numberOfSellers)
      {
         return Enumerable.Range(1, numberOfSellers)
                          .Select(i => new Seller
                                       {
                                          Id = i,
                                          Name = $"Seller {i}"
                                       })
                          .ToList();
      }

      private List<Seller_Product> GenerateSellerProducts(
         List<Product> products,
         List<Seller> sellers)
      {
         var sellersProduct = new List<Seller_Product>();

         foreach (var product in products)
         foreach (var seller in sellers)
         {
            sellersProduct.Add(new Seller_Product
                               {
                                  ProductId = product.Id,
                                  SellerId = seller.Id
                               });
         }

         return sellersProduct;
      }

      private List<Studio> GenerateStudio(int numberOfStudios)
      {
         return Enumerable.Range(1, numberOfStudios)
                          .Select(i => new Studio
                                       {
                                          Id = i,
                                          Name = $"Studio {i}"
                                       })
                          .ToList();
      }

      private List<ProductGroup> GenerateProductGroups(int numberOfGroups)
      {
         return Enumerable.Range(1, numberOfGroups)
                          .Select(i => new ProductGroup
                                       {
                                          Id = i
                                       })
                          .ToList();
      }

      private List<Product> GenerateProducts(
         List<ProductGroup> groups,
         List<Studio> studios,
         int numberOfProducts)
      {
         return Enumerable.Range(1, numberOfProducts)
                          .Select(i => new Product
                                       {
                                          Id = i,
                                          Name = $"Product {i}",
                                          DeliverableFrom = new DateTime(2000, 01, 01),
                                          DeliverableUntil = new DateTime(2030, 01, 01),
                                          ProductGroupId = groups[i % groups.Count].Id,
                                          StudioId = studios[i % studios.Count].Id
                                       })
                          .ToList();
      }
   }
}
