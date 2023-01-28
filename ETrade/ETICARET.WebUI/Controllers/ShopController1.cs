﻿using ETICARET.Business.Abstract;
using ETICARET.Entities;
using ETICARET.WebUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace ETICARET.WebUI.Controllers
{
    public class ShopController1 : Controller
    {

        private IProductService _productService;

        public ShopController1(IProductService productService)
        {
            _productService = productService;
        }

        // products/telefon
        [Route("products/{category?}")]
        public IActionResult List(string category, int page = 1)
        {
            const int pageSize = 3;

            var products = new ProductListModel()
            {
                PageInfo = new PageInfo()
                {
                    TotalItems = _productService.GetCountByCategory(category),
                    ItemsPerPage = pageSize,
                    CurrentCategory = category,
                    CurrentPage = page
                },
                Products = _productService.GetProductsByCategory(category,page,pageSize)
            };


            return View(products);
        }

        public IActionResult Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            Product product = _productService.GetProductDetails(id.Value);

            if(product == null)
            {
                NotFound();
            }

            return View(
                new ProductDetailsModel()
                {
                    Product = product,
                    Categories = product.ProductCategories.Select(i => i.Category).ToList(),
                    Comments = product.Comments
                }    
            );
        }
    }
}
