using Microsoft.AspNetCore.Mvc;
using Fridge_app.Models;
using Fridge_app.Services;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;
using System.IO;
using System.Linq;
using Fridge_app.Models.Mappers;
using Microsoft.EntityFrameworkCore;

public class ProductController : Controller
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }
    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllProductsAsync();
        return View(products);
    }
    public IActionResult Create()
    {
        ViewBag.Units = new List<string> { "pcs", "kg", "liter" };

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
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError("", "Proszę wybrać plik.");
            return View();
        }

        List<Product> products = new List<Product>();
        try
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = false,
                MissingFieldFound = null
            };

            using (var reader = new StreamReader(file.OpenReadStream()))
            using (var csv = new CsvReader(reader, config))
            {
                csv.Context.RegisterClassMap<ProductCsvMap>();
                products = csv.GetRecords<Product>().ToList();
            }

            foreach (var product in products)
            {
                product.Unit = "g";
                product.ProductCategory = ProductCategory.Other;
                product.PriceMin = 0;
                product.PriceMax = 0;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Błąd podczas przetwarzania pliku: {ex.Message}");
            return View();
        }

        return View("Preview", products);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(List<Product> products)
    {
        if (ModelState.IsValid)
        {
            foreach (var product in products)
            {
                await _productService.AddProductAsync(product);
            }
            return RedirectToAction(nameof(Index));
        }
        return View("Preview", products);
    }
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var product = await _productService.GetProductAsync(id.Value);
        if (product == null)
        {
            return NotFound();
        }

        ViewBag.Units = new List<string> { "pcs", "kg", "liter", "g" };
        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Product product)
    {
        if (id != product.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _productService.UpdateProductAsync(product);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        ViewBag.Units = new List<string> { "pcs", "kg", "liter", "g" };
        return View(product);
    }

    private async Task<bool> ProductExists(int id)
    {
        return (await _productService.GetProductAsync(id)) != null;
    }
}

