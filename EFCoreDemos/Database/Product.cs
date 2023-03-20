namespace EFCoreDemos.Database;

public class Product
{
   public int Id { get; set; }
   public string Name { get; set; }

   public DateTime DeliverableFrom { get; set; }
   public DateTime DeliverableUntil { get; set; }

   public int StudioId { get; set; }
   public virtual Studio Studio { get; set; }

   public virtual List<Price> Prices { get; set; }
   public virtual List<Seller> Sellers { get; set; }
}
