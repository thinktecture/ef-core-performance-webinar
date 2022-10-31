namespace EFCoreDemos.Database;

public class Seller
{
   public int Id { get; set; }
   public string Name { get; set; }

   public virtual List<Product> Products { get; set; }
}