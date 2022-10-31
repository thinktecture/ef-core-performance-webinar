namespace EFCoreDemos.Database;

public class ProductGroup
{
   public int Id { get; set; }

   public virtual List<Product> Products { get; set; }
}