using Microsoft.AspNetCore.Mvc;
using TCSA_Budget.Arashi256.Models;
using TCSA_Budget.Arashi256.Interfaces;

namespace TCSA_Budget.Arashi256.Controllers
{
    public class TransactionController : Controller
    {
        private readonly IRepository<Transaction> _transactionRepo;
        private readonly IRepository<Category> _categoryRepo;

        public TransactionController(IRepository<Transaction> transactionRepo, IRepository<Category> categoryRepo)
        {
            _transactionRepo = transactionRepo;
            _categoryRepo = categoryRepo;
        }

        // GET: List all transactions.
        public async Task<IActionResult> Index(string searchTerm = "", int? categoryId = null, string? fromDate = null, string? toDate = null)
        {
            var transactions = (await _transactionRepo.GetAll())
                .Select(t => new Transaction
                {
                    Id = t.Id,
                    Description = t.Description,
                    Amount = t.Amount,
                    Date = t.Date,
                    CategoryId = t.CategoryId,
                    Category = t.Category ?? new Category { Name = "Unknown" } // Ensures Category is always set.
                }).ToList();
            // Ensure categories are loaded.
            foreach (var transaction in transactions)
            {
                transaction.Category = await _categoryRepo.GetById(transaction.CategoryId);
            }
            // Filter by category if selected.
            if (categoryId.HasValue && categoryId.Value != 0)
            {
                transactions = transactions.Where(t => t.CategoryId == categoryId.Value).ToList();
            }
            // Filter by date range if provided.
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out DateTime fromDateParsed))
            {
                transactions = transactions.Where(t => t.Date >= fromDateParsed).ToList();
            }
            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out DateTime toDateParsed))
            {
                transactions = transactions.Where(t => t.Date <= toDateParsed).ToList();
            }
            // Filter by search term.
            if (!string.IsNullOrEmpty(searchTerm))
            {
                transactions = transactions.Where(t => t.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            // Fetch categories for the dropdown.
            var categories = await _categoryRepo.GetAll();
            ViewData["Categories"] = categories;
            return View(transactions);
        }


        // GET: Create Transaction Modal (Partial View).
        public async Task<IActionResult> Create()
        {
            ViewData["Categories"] = await _categoryRepo.GetAll();
            return PartialView("_CreateTransaction", new Transaction());
        }

        // POST: Create Transaction.
        [HttpPost]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            var categories = await _categoryRepo.GetAll(); // Fetch categories.
            ViewData["Categories"] = categories; // Ensure categories are available.
            // Check for duplicate transactions.
            if (await IsDuplicateTransaction(transaction.Id, transaction.Description, transaction.Amount, transaction.CategoryId, transaction.Date))
            {
                ModelState.AddModelError(string.Empty, "A transaction with these details already exists");
                TempData["ErrorMessage"] = "A transaction with these details already exists";
                ViewData["ShowCreateModal"] = true; // Keep modal open.
            }
            // If ModelState is invalid OR a duplicate exists, return to Index with errors.
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add transaction";
                return View("Index", await GetTransactionList());
            }
            // Add new transaction.
            await _transactionRepo.Add(transaction);
            TempData["SuccessMessage"] = "Transaction added successfully";
            // Refresh the Index view with updated transactions list.
            return View("Index", await GetTransactionList());
        }

        // Get a list of transactions with categories loaded.
        private async Task<List<Transaction>> GetTransactionList()
        {
            var transactions = await _transactionRepo.GetAll();
            foreach (var transaction in transactions)
            {
                transaction.Category = await _categoryRepo.GetById(transaction.CategoryId); // Ensure category is loaded.
            }
            return transactions.ToList();
        }

        // Check for duplicate transactions.
        private async Task<bool> IsDuplicateTransaction(int id, string description, decimal amount, int categoryId, DateTime date)
        {
            var transactions = await _transactionRepo.GetAll();
            return transactions.Any(t =>
                t.Id != id && // Exclude itself.
                t.Description == description &&
                t.Amount == amount &&
                t.CategoryId == categoryId &&
                t.Date == date);
        }

        // GET: Edit Transaction Modal (Partial View)
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _transactionRepo.GetById(id);
            if (transaction == null) return NotFound();
            // Ensure Category is explicitly loaded.
            transaction.Category = await _categoryRepo.GetById(transaction.CategoryId) ?? new Category { Name = "Unknown" };
            // Always ensure Categories are loaded for dropdown.
            ViewData["Categories"] = await _categoryRepo.GetAll();
            return PartialView("_EditTransaction", transaction);
        }

        // POST: Edit Transaction
        [HttpPost]
        public async Task<IActionResult> Edit(Transaction transaction)
        {
            // Always reload categories for dropdown in case of validation errors.
            ViewData["Categories"] = await _categoryRepo.GetAll();
            // Check for duplicate transactions.
            if (await IsDuplicateTransaction(transaction.Id, transaction.Description, transaction.Amount, transaction.CategoryId, transaction.Date))
            {
                ModelState.AddModelError(string.Empty, "A transaction with these details already exists.");
                ViewData["ShowEditModal"] = transaction.Id; // Keep the modal open.
                var transactions = (await _transactionRepo.GetAll()).ToList();
                transactions.ForEach(t => t.Category = _categoryRepo.GetById(t.CategoryId).Result ?? new Category { Name = "Unknown" });
                TempData["ErrorMessage"] = "A transaction with these details already exists";
                return View("Index", transactions);
            }
            if (!ModelState.IsValid)
            {
                ViewData["ShowEditModal"] = transaction.Id;
                var transactions = (await _transactionRepo.GetAll()).ToList();
                transactions.ForEach(t => t.Category = _categoryRepo.GetById(t.CategoryId).Result ?? new Category { Name = "Unknown" });
                TempData["ErrorMessage"] = "Could not update transaction";
                return View("Index", transactions);
            }
            // Ensure Entity Tracking Doesn't Cause Issues.
            var existingTransaction = await _transactionRepo.GetById(transaction.Id);
            if (existingTransaction == null) return NotFound();
            existingTransaction.Description = transaction.Description;
            existingTransaction.CategoryId = transaction.CategoryId;
            existingTransaction.Amount = transaction.Amount;
            existingTransaction.Date = transaction.Date;
            await _transactionRepo.Update(existingTransaction);
            TempData["SuccessMessage"] = "Transaction updated successfully";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete Confirmation Modal (Partial View).
        public async Task<IActionResult> Delete(int id)
        {
            var transaction = await _transactionRepo.GetById(id);
            if (transaction == null) return NotFound();
            return PartialView("_DeleteTransaction", transaction);
        }

        // POST: Delete Transaction.
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _transactionRepo.GetById(id);
            if (transaction == null)
            {
                TempData["ErrorMessage"] = "Transaction not found.";
                return RedirectToAction(nameof(Index));
            }
            await _transactionRepo.Delete(id);
            TempData["SuccessMessage"] = "Transaction deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}