namespace EFCoreDemos.Database;

public class Seller_Product
{
   public int SellerId { get; set; }
   public virtual Seller Seller { get; set; }

   public int ProductId { get; set; }
   public virtual Product Product { get; set; }
}