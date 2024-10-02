using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce_Website.Controllers
{
    public class CustomerLoginController : Controller
    {
        private readonly myContext _context;

        public CustomerLoginController(myContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string customer_email, string customer_password)
        {
            var existingCustomer = _context.tbl_customer
                .FirstOrDefault(c => c.customer_email == customer_email && c.customer_password == customer_password);

            if (existingCustomer != null)
            {
                HttpContext.Session.SetString("CustomerEmail", existingCustomer.customer_email);
                return RedirectToAction("Dashboard", "Customer");
            }

            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        public IActionResult Register()
        {
            return View(new Customer());
        }

        [HttpPost]
        public IActionResult Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.tbl_customer.Add(customer);
                _context.SaveChanges();

                HttpContext.Session.SetString("CustomerEmail", customer.customer_email);
                return RedirectToAction("Dashboard", "Customer");
            }

            return View(customer);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
