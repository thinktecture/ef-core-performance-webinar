using EFCoreDemos.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EFCoreDemos;

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
      // Approach_2_2_with_AsSplitQuery();
      // Approach_2_2_manual_query_splitting();

      // Demo_3_1();
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

      var pricesByProductId = GetPrices(productIds);

      foreach (var product in products)
      {
         product.Prices = pricesByProductId[product.Id].ToList();
      }
   }

   private ILookup<int, Price> GetPrices(IEnumerable<int> productIds)
   {
      return _ctx.Prices
                 .Where(p => productIds.Contains(p.ProductId))
                 .ToLookup(p => p.ProductId);
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
      var result = _ctx.Studios
                       .Select(s => new
                                    {
                                       Studio = s,
                                       Infinity = s.Products
                                                   .Where(p => p.Name.Contains("Infinity"))
                                                   .Select(p => new
                                                                {
                                                                   Product = p,
                                                                   p.Prices,
                                                                   p.Sellers
                                                                }),
                                       Endgame = s.Products
                                                  .Where(p => p.Name.Contains("Endgame"))
                                                  .Select(p => new
                                                               {
                                                                  Product = p,
                                                                  p.Prices,
                                                                  p.Sellers
                                                               })
                                    })
                       .ToList();
   }

   private void Approach_2_2_with_AsSplitQuery()
   {
      var result = _ctx.Studios
                       .AsSplitQuery()
                       .Select(s => new
                                    {
                                       Studio = s,
                                       Infinity = s.Products
                                                   .Where(p => p.Name.Contains("Infinity"))
                                                   .Select(p => new
                                                                {
                                                                   Product = p,
                                                                   p.Prices,
                                                                   p.Sellers
                                                                }),
                                       Endgame = s.Products
                                                  .Where(p => p.Name.Contains("Endgame"))
                                                  .Select(p => new
                                                               {
                                                                  Product = p,
                                                                  p.Prices,
                                                                  p.Sellers
                                                               })
                                    })
                       .ToList();
   }

   private void Approach_2_2_manual_query_splitting()
   {
      var studiosQuery = _ctx.Studios;
      var studios = studiosQuery.ToList();
      var infinityProducts = studiosQuery.SelectMany(s => s.Products).Where(p => p.Name.Contains("Infinity")).ToList();
      var endgameProducts = studiosQuery.SelectMany(s => s.Products).Where(p => p.Name.Contains("Endgame")).ToList();
      var productIds = infinityProducts.Concat(endgameProducts).Select(p => p.Id);
      var prices = _ctx.Prices.Where(p => productIds.Contains(p.ProductId)).ToList();

      // cannot use "productsQuery.Select(p => p.Sellers)" because JoinTable "Seller_Product" won't be selected,
      // thus Change Tracker won't wire up the property "Sellers" on the products.
      var sellers = _ctx.SellerProducts.Where(sp => productIds.Contains(sp.ProductId))
                        .Include(sp => sp.Seller)
                        .ToList();

      // Build the desired data structure in memory
      var result = studios
                   .Select(s => new
                                {
                                   Studio = s,
                                   // "Products" is populated thanks to ChangeTracking
                                   Infinity = s.Products
                                               .Where(p => p.Name.Contains("Infinity"))
                                               .Select(p => new
                                                            {
                                                               Product = p,
                                                               p.Prices,
                                                               p.Sellers
                                                            }),
                                   Endgame = s.Products
                                              .Where(p => p.Name.Contains("Endgame"))
                                              .Select(p => new
                                                           {
                                                              Product = p,
                                                              p.Prices,
                                                              p.Sellers
                                                           })
                                })
                   .ToList();
   }

   private void Demo_3_1()
   {
      var results1 = _ctx.Studios
                         .Select(s => new
                                      {
                                         s.Products.OrderBy(p => p.Id).FirstOrDefault().Id,
                                         s.Products.OrderBy(p => p.Id).FirstOrDefault().Name
                                      })
                         .ToList();

      var results2 = _ctx.Studios
                         .Select(s => s.Products
                                       .OrderBy(p => p.Id)
                                       .Select(p => new
                                                    {
                                                       p.Id,
                                                       p.Name
                                                    })
                                       .FirstOrDefault())
                         .ToList();
   }
}
