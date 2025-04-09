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

