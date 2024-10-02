using Microsoft.AspNetCore.Mvc;
using E_Commerce_Website.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Website.Controllers
{
    public class CategoryController : Controller
    {
        private readonly myContext _context;

        public CategoryController(myContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.tbl_category.ToList();
            return View(categories);
        }

    
        public IActionResult Products(int categoryId)
        {
            var products = _context.tbl_product
                .Where(p => p.cat_id == categoryId)
                .Include(p => p.Category)
                .ToList();

            return View("dashboard", products);
        }
    }
}
