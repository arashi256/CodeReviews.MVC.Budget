using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCSA_Budget.Arashi256.Interfaces;
using TCSA_Budget.Arashi256.Models;

namespace TCSA_Budget.Arashi256.Controllers
{
    public class CategoryController : Controller
    {
        private readonly IRepository<Category> _categoryRepo;

        public CategoryController(IRepository<Category> categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        // GET: /Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepo.GetAll();
            return View(categories);
        }

        // GET: Create Modal (Partial View)
        public IActionResult Create()
        {
            return PartialView("_CreateCategory");
        }

        // POST: Create Category
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            try
            {
                if (await IsDuplicateCategory(category.Name)) // Check in EF before hitting DB.
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    ViewData["ShowCreateModal"] = true; // Keep the modal open.
                    var categories = await _categoryRepo.GetAll();
                    return View("Index", categories.ToList());
                }
                await _categoryRepo.Add(category);
                TempData["SuccessMessage"] = "Category added successfully";
                return RedirectToAction(nameof(Index)); // Success.
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Categories_Name") == true)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    ViewData["ShowCreateModal"] = true; // Keep the modal open.
                    var categories = await _categoryRepo.GetAll();
                    return View("Index", categories.ToList()); // Return Index to show error.
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                }
            }
            // Ensure a response is always returned.
            ViewData["ShowCreateModal"] = true;
            var allCategories = await _categoryRepo.GetAll();
            TempData["ErrorMessage"] = "Failed to add category";
            return View("Index", allCategories.ToList());
        }

        private async Task<bool> IsDuplicateCategory(string name, int? categoryId = null)
        {
            var categories = await _categoryRepo.GetAll(); // Fetch all categories.
            return categories.Any(c => c.Name == name && (categoryId == null || c.Id != categoryId));
        }

        // GET: Edit Modal (Partial View)
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepo.GetById(id);
            if (category == null) return NotFound();
            return PartialView("_EditCategory", category);
        }

        // POST: Edit Category
        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            try
            {
                if (await IsDuplicateCategory(category.Name, category.Id) || !ModelState.IsValid)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    ViewData["ShowEditModal"] = category.Id; // Keep the modal open.
                    var categories = await _categoryRepo.GetAll();
                    return View("Index", categories.ToList()); // Return to Index to show error.
                }
                await _categoryRepo.Update(category);
                TempData["SuccessMessage"] = "Category updated successfully";
                return RedirectToAction(nameof(Index)); // Success.
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("IX_Categories_Name") == true)
                {
                    ModelState.AddModelError("Name", "A category with this name already exists.");
                    ViewData["ShowEditModal"] = category.Id; // Keep the modal open.
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                }
                TempData["ErrorMessage"] = "Failed to update category";
            }
            // Ensure a response is always returned.
            ViewData["ShowEditModal"] = category.Id;
            var allCategories = await _categoryRepo.GetAll();
            return View("Index", allCategories.ToList());
        }

        // GET: Delete Confirmation Modal (Partial View).
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryRepo.GetById(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Failed to delete category";
                return NotFound();
            }
            return PartialView("_DeleteCategory", category);
        }

        // POST: Delete Category.
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _categoryRepo.Delete(id);
            TempData["SuccessMessage"] = "Category deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}