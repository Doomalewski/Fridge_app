// Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Fridge_app.Models;
using Fridge_app.Services;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;

public class ProductsController : Controller
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    public IActionResult Create()
    {
        ViewBag.Units = new List<string> { "pcs", "kg", "liter" };
        var processor = new CsvProcessor();
        processor.ProcessCsv("C:\\en.openfoodfacts.org.products.csv");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            await _productService.AddProductAsync(product);
            return RedirectToAction(nameof(Index));
        }
        return View(product);
    }

}
public class Producttest
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public string IngredientsText { get; set; }
    public double? EnergyKj100g { get; set; }
    public double? EnergyKcal100g { get; set; }
    public double? Fat100g { get; set; }
    public double? SaturatedFat100g { get; set; }
    public double? Carbohydrates100g { get; set; }
    public double? Sugars100g { get; set; }
    public double? Proteins100g { get; set; }
    public double? Salt100g { get; set; }
    // Dodaj inne właściwości według potrzeb
}

public sealed class ProductMap : ClassMap<Producttest>
{
    public ProductMap()
    {
        Map(m => m.ProductCode).Name("code");
        Map(m => m.ProductName).Name("product_name");
        Map(m => m.IngredientsText).Name("ingredients_text");
        Map(m => m.EnergyKj100g).Name("energy-kj_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.EnergyKcal100g).Name("energy-kcal_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.Fat100g).Name("fat_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.SaturatedFat100g).Name("saturated-fat_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.Carbohydrates100g).Name("carbohydrates_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.Sugars100g).Name("sugars_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.Proteins100g).Name("proteins_100g").TypeConverter<NullableDoubleConverter>();
        Map(m => m.Salt100g).Name("salt_100g").TypeConverter<NullableDoubleConverter>();
        // Dodaj mapowania dla innych kolumn
    }
}

public class NullableDoubleConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;
        if (text.Equals("unknown", StringComparison.OrdinalIgnoreCase)) return null;

        if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return null;
    }
}

public class CsvProcessor
{
    public void ProcessCsv(string filePath)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "\t", // Ustaw separator na tabulację
            PrepareHeaderForMatch = args => args.Header.ToLower(),
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null
        };

        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            csv.Context.RegisterClassMap<ProductMap>();
            var products = csv.GetRecords<Producttest>().ToList();

            // Przetwarzaj produkty (np. zapisz do bazy danych)
            foreach (var product in products)
            {
                Console.WriteLine($"Product: {product.ProductName}, Kalorie: {product.EnergyKcal100g}");
            }
        }
    }
}