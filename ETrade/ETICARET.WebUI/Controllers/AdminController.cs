﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ETICARET.Business.Abstract;
using Microsoft.AspNetCore.Identity;
using ETICARET.WebUI.Identity;
using ETICARET.WebUI.Models;
using ETICARET.Entities;

namespace ETICARET.WebUI.Controllers
{
    [Authorize(Roles = "admin")]
    public class AdminController : Controller
    {
        IProductService _productService;
        ICategoryService _categoryService;
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;

        public AdminController(IProductService productService, ICategoryService categoryService, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _productService = productService;
            _categoryService = categoryService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Route("admin/products")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult ProductList()
        {
            return View(new ProductListModel()
            {
                Products = _productService.GetAll()
            });
        }

        public IActionResult CreateProduct()
        {
            return View(new ProductModel());
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(ProductModel model, List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                var entity = new Product()
                {
                    Name = model.Name,
                    Description = model.Description,
                    Price = model.Price
                };

                if(files != null)
                {
                    foreach (var file in files)
                    {
                        Image image = new Image();
                        image.ImageUrl = file.FileName;

                        entity.Images.Add(image);

                        var path = Path.Combine(Directory.GetCurrentDirectory(),"wwwroot\\img",file.FileName);

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }

                _productService.Create(entity);

                return Redirect("/admin/products");
            }
            return View(model);
        }

        public IActionResult EditProduct(int id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var entity = _productService.GetProductDetails(id);

            if(entity == null)
            {
                return NotFound();
            }

            var model = new ProductModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Images = entity.Images,
                SelectedCategory = entity.ProductCategories.Select(i => i.Category).ToList()
            };

            ViewBag.Categories = _categoryService.GetAll();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(ProductModel model, List<IFormFile> files, int[] categoryIds)
        {
            var entity = _productService.GetById(model.Id);

            if(entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            entity.Description = model.Description;
            entity.Price = model.Price;

            if (files != null)
            {
                foreach (var file in files)
                {
                    Image image = new Image();
                    image.ImageUrl = file.FileName;

                    entity.Images.Add(image);

                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\img", file.FileName);

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
                _productService.Update(entity, categoryIds);
                return RedirectToAction("ProductList");

            }

            return View(entity);

        }

        public IActionResult DeleteProduct(int productId)
        {
            var product = _productService.GetById(productId);

            _productService.Delete(product);

            return RedirectToAction("ProductList");
        }

        [Route("admin/categories")]
        public IActionResult CategoryList()
        {
            return View(new CategoryListModel()
            {
                Categories = _categoryService.GetAll()
            });
        }

        public IActionResult CreateCategory()
        {
            return View(new CategoryModel());
        }

        [HttpPost]
        public IActionResult CreateCategory(CategoryModel model)
        {
            var entity = new Category()
            {
                Name = model.Name
            };

            _categoryService.Create(entity);
            return RedirectToAction("CategoryList");
        }

        public IActionResult EditCategory(int? id)
        {
            var entity = _categoryService.GetByIdWithProducts(id.Value); // (int)id

            return View(new CategoryModel()
            {
                Id = entity.Id,
                Name = entity.Name,
                Products = entity.ProductCategories.Select(i => i.Product).ToList()
            });
        }

        [HttpPost]
        public IActionResult EditCategory(CategoryModel model)
        {
            var entity = _categoryService.GetById(model.Id);

            if (entity == null)
            {
                return NotFound();
            }

            entity.Name = model.Name;
            _categoryService.Update(entity);
            return RedirectToAction("CategoryList");
    
        }

        [HttpPost]
        public IActionResult DeleteCategory(int categoryId)
        {
            var entity = _categoryService.GetById(categoryId);

            _categoryService.Delete(entity);

            return RedirectToAction("CategoryList");
        }

        [HttpPost]
        public IActionResult DeleteFromCategory(int categoryId, int productId)
        {
            _categoryService.DeleteFromCategory(categoryId,productId);

            return Redirect("admin/categories/"+categoryId);
        }

        public async Task<IActionResult> UserList()
        {
            List<ApplicationUser> userList = _userManager.Users.ToList();
            List<AdminUserModel> model = new List<AdminUserModel>();

            foreach (ApplicationUser item in userList)
            {
                AdminUserModel user = new AdminUserModel();

                user.FullName = item.FullName;
                user.Username = item.UserName;
                user.EmailConfirmed = item.EmailConfirmed;
                user.Email = item.Email;
                user.IsAdmin = await _userManager.IsInRoleAsync(item,"admin");

                model.Add(user);

            }

            return View(model);
        }

        public async Task<IActionResult> UserEdit(string Email) 
        {
            ApplicationUser entity = await _userManager.FindByEmailAsync(Email);

            if(entity == null)
            {
                ModelState.AddModelError("","Bu kullanıcı ile daha önce hesap oluşturulmamıştır");

                return View(entity);
            }

            AdminUserModel user = new AdminUserModel()
            {
                FullName = entity.FullName,
                Username = entity.UserName,
                EmailConfirmed = entity.EmailConfirmed,
                Email = entity.Email,
                IsAdmin = await _userManager.IsInRoleAsync(entity,"admin")
            };

            return View(user);
        
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(AdminUserModel model)
        {
            ApplicationUser entity = await _userManager.FindByEmailAsync(model.Email);

            if (entity == null)
            {
                ModelState.AddModelError("", "Bu kullanıcı ile daha önce hesap oluşturulmamıştır");

                return View(entity);
            }

            entity.FullName = model.FullName;
            entity.EmailConfirmed = model.EmailConfirmed;
            entity.Email = model.Email;
            if (model.IsAdmin)
            {
                await _userManager.AddToRoleAsync(entity,"admin");
            }
            else
            {
                await _userManager.RemoveFromRoleAsync(entity, "admin");
            }

            await _userManager.UpdateAsync(entity);

            return RedirectToAction("UserList","Admin");

        }

        [HttpPost]
        public async Task<IActionResult> UserDelete(string Email)
        {
            ApplicationUser entity = await _userManager.FindByEmailAsync(Email);

            if(entity != null)
            {
                await _userManager.DeleteAsync(entity);

                return RedirectToAction("UserList","Admin");
            }

            ModelState.AddModelError("","Silme işlemi başarısız!");

            return View(entity);
        }
    }
}
