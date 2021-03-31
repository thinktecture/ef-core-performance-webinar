using System;
using System.Collections.Generic;
using System.Linq;
using EFCoreDemos.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemos
{
   public class Demos
   {
      private readonly ILogger<Demos> _logger;
      private readonly DemoDbContext _ctx;

      public Demos(
         ILogger<Demos> logger,
         DemoDbContext ctx)
      {
         _logger = logger ?? throw new ArgumentNullException(nameof(logger));
         _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
      }

      public void Execute()
      {
         // The following demos are using the sync-API for simplicity reasons

         // Demo_1_1();
         // Demo_1_2();
         //
         // Approach_1_1();
         // Approach_1_2();
         //
         // Demo_1_3();
         // Approach_1_3();
         //
         // Demo_2_1();
         // Approach_2_1();
         //
         // Demo_2_2();
         // Approach_2_2();
         // Approach_2_2_without_AsSplitQuery();
      }

      private void Demo_1_1()
      {
         var products = _ctx.Products.ToList();

         _logger.LogInformation("{NumberOfProducts} products loaded", products.Count);

         var prices = new List<Price>();

         foreach (var product in products)
         {
            var price = GetPrice(product.Id);
            prices.Add(price);
         }
      }

      private Price GetPrice(int productId)
      {
         // oversimplified

         return _ctx.Prices.FirstOrDefault(p => p.ProductId == productId);
      }

      private void Demo_1_2()
      {
         var products = _ctx.Products.ToList();

         _logger.LogInformation("{NumberOfProducts} products loaded", products.Count);

         var prices = products.Select(product => GetPrice(product.Id))
                              .ToList();
      }

      private void Approach_1_1()
      {
         // Use-case specific method

         var productsWithPrices = _ctx.Products
                                      // merely symbolic, fetching prices is usually more complex
                                      .Include(p => p.Prices)
                                      .ToList();
      }

      private void Approach_1_2()
      {
         var products = _ctx.Products.ToList();

         var productIds = products.Select(p => p.Id);

         var prices = GetPrices(productIds);
      }

      private List<Price> GetPrices(IEnumerable<int> productIds)
      {
         return _ctx.Prices
                    .Where(p => productIds.Contains(p.ProductId))
                    .ToList();
      }

      private void Demo_1_3()
      {
         // Activate lazy-loading!

         var productsByStudio = _ctx.Products
                                    .ToLookup(p => p.Studio.Name);
      }

      private void Approach_1_3()
      {
         // Activate lazy-loading!

         var productsByStudio = _ctx.Products
                                    .Include(p => p.Studio)
                                    .ToLookup(p => p.Studio.Name);
      }

      private void Demo_2_1()
      {
         var products = _ctx.Products
                            .Include(p => p.Studio)
                            .Include(p => p.Prices)
                            .Include(p => p.Sellers)
                            .ToList();
      }

      private void Approach_2_1()
      {
         var products = _ctx.Products
                            .AsSplitQuery()
                            .Include(p => p.Studio)
                            .Include(p => p.Prices)
                            .Include(p => p.Sellers)
                            .ToList();
      }

      private void Demo_2_2()
      {
         var result = _ctx.ProductGroups
                          // .AsSplitQuery() // leads to an InvalidOperationException
                          .Select(g => new
                                       {
                                          Group = g,
                                          Products = g.Products
                                                      .Select(p => new
                                                                   {
                                                                      Product = p,
                                                                      p.Prices,
                                                                      p.Sellers
                                                                   })
                                       })
                          .ToList();
      }

      private void Approach_2_2()
      {
         var groupQuery = _ctx.ProductGroups; // may have additional filters

         var groups = groupQuery.ToList();

         var products = groupQuery.SelectMany(g => g.Products)
                                  .AsSplitQuery()
                                  .Include(p => p.Prices)
                                  .Include(p => p.Sellers)
                                  .ToList();

         // Build the desired data structure
         var result = groups
                      // .AsSplitQuery() // leads to an InvalidOperationException
                      .Select(g => new
                                   {
                                      Group = g,
                                      Products = g.Products // "Products" is populated thanks to ChangeTracking
                                                  .Select(p => new
                                                               {
                                                                  Product = p,
                                                                  p.Prices,
                                                                  p.Sellers
                                                               })
                                   })
                      .ToList();
      }

      // Alternative way without "AsSplitQuery" (which was the only way before EF 5)
      private void Approach_2_2_without_AsSplitQuery()
      {
         var groupQuery = _ctx.ProductGroups; // may have additional filters
         var groups = groupQuery.ToList();

         var productsQuery = groupQuery.SelectMany(g => g.Products); // may have additional filters
         var products = productsQuery.ToList();                      // Alternatively, we can fetch the Products along with the ProductGroups

         var prices = productsQuery.SelectMany(p => p.Prices).ToList();

         // cannot use "productsQuery.Select(p => p.Sellers)" because JoinTable "Seller_Product" won't be selected
         var sellers = _ctx.Set<Seller_Product>()
                           .Join(productsQuery, sp => sp.ProductId, p => p.Id, (sp, p) => sp)
                           .Include(sp => sp.Seller)
                           .ToList();

         // Build the desired data structure
         var result = groups
                      // .AsSplitQuery() // leads to an InvalidOperationException
                      .Select(g => new
                                   {
                                      Group = g,
                                      Products = g.Products // "Products" is populated thanks to ChangeTracking
                                                  .Select(p => new
                                                               {
                                                                  Product = p,
                                                                  p.Prices,
                                                                  p.Sellers
                                                               })
                                   })
                      .ToList();
      }
   }
}
