using CsvHelper.Configuration;

namespace Fridge_app.Models.Mappers
{
    public sealed class ProductCsvMap : ClassMap<Product>
    {
        public ProductCsvMap()
        {
            Map(m => m.Name).Index(0);
            Map(m => m.Kcal).Index(1);
            Map(m => m.Fat).Index(2);
            Map(m => m.UnsaturatedFat).Index(3);
            Map(m => m.Carbohydrates).Index(4);
            Map(m => m.Sugar).Index(5);
            Map(m => m.Protein).Index(6);
            Map(m => m.Fiber).Index(7);
            Map(m => m.Salt).Index(8);
            Map(m => m.PriceMin).Ignore();
            Map(m => m.PriceMax).Ignore();
            Map(m => m.Unit).Ignore();
            Map(m => m.ProductCategory).Ignore();
        }
    }
}
