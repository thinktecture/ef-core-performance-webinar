namespace EFCoreDemos.Database;

public class Studio
{
   public int Id { get; set; }
   public string Name { get; set; }

   public List<Product> Products { get; set; }
}
