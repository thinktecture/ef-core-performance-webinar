using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

         Demo_1_1();
         Demo_1_2();

         Approach_1_1();
         Approach_1_2();

         Demo_1_3();
         Approach_1_3();

         Demo_1_4();
         Approach_1_4();

         Demo_2_1();
         Approach_2_1();
      }

      private void Demo_1_1()
      {
         var products = _ctx.Products.ToList();

         _logger.LogInformation("{NumberOfProducts} products loaded.", products.Count);

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

         _logger.LogInformation("{NumberOfProducts} products loaded.", products.Count);

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

      private void Demo_1_4()
      {
         try
         {
            var products = _ctx.Products
                               .Where(p => IsDeliverable(p))
                               .ToList();
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, $"The method '{nameof(IsDeliverable)}' cannot be translated to SQL.");
         }
      }

      private bool IsDeliverable(Product product)
      {
         return product.DeliverableFrom <= DateTime.Now &&
                product.DeliverableUntil > DateTime.Now;
      }

      private void Approach_1_4()
      {
         var products = _ctx.Products
                            .Where(IsDeliverable())
                            .ToList();
      }

      private Expression<Func<Product, bool>> IsDeliverable()
      {
         return p => p.DeliverableFrom <= DateTime.Now &&
                     p.DeliverableUntil > DateTime.Now;
      }

      private void Demo_2_1()
      {
         var products = _ctx.Products
                            .Include(p => p.Studio)
                            .Include(p => p.Prices)
                            .Include(p => p.SellerProducts)
                            .ThenInclude(s => s.Seller)
                            .ToList();
      }

      private void Approach_2_1()
      {
         var products = _ctx.Products
                            .Include(p => p.Studio)
                            .ToList();

         var productIds = products.Select(p => p.Id);

         _ctx.Prices.Where(p => productIds.Contains(p.ProductId)).ToList();
         _ctx.SellerProducts.Include(sp => sp.Seller).Where(p => productIds.Contains(p.ProductId)).ToList();
      }
   }
}
