﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineShop.Data;
using OnlineShop.Services.Data;
using OnlineShop.Services.Data.Interfaces;
using OnlineShop.Web.ViewModels.Product;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OnlineShop.Data.Models;

namespace OnlineShop.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;
        public ProductController(ApplicationDbContext context, IProductService productService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _productService = productService;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {

            var viewModel = new CreateProductViewModel
            {
                Genders = await _productService.GetGendersAsync(),
                ClothingTypes = await _productService.GetClothingTypesAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductViewModel product)
        {
            ModelState.Remove(nameof(product.Genders));
            ModelState.Remove(nameof(product.ClothingTypes));

            if (!ModelState.IsValid)
            {
                product.Genders = await _productService.GetGendersAsync();
                product.ClothingTypes = await _productService.GetClothingTypesAsync();

                return View(product);
            }

            await _productService.CreateProductAsync(product, GetUserId()!);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);
            var product = await _productService.GetEditProductViewModelAsync(id, userId!);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ModelState.Remove(nameof(model.Genders));
            ModelState.Remove(nameof(model.ClothingTypes));

            if (!ModelState.IsValid)
            {
                model.Genders = await _productService.GetGendersAsync();
                model.ClothingTypes = await _productService.GetClothingTypesAsync();
                return View(model);
            }

            var userId = _userManager.GetUserId(User);

            var success = await _productService.UpdateProductAsync(model, userId);

            if (!success)
            {
                return Forbid();
            }

            return RedirectToAction("Details", new { id = model.Id });
        }


        public string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }
    }
}
