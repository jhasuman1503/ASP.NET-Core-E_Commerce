using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace E_Commerce_Website.Controllers
{
    public class CustomerController : Controller
    {
        private readonly myContext _context;

        public CustomerController(myContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Customer customer)
        {
            if (ModelState.IsValid)
            {
                if (_context.tbl_customer.Any(c => c.customer_email == customer.customer_email))
                {
                    ModelState.AddModelError("", "An account with this email already exists.");
                    return View(customer);
                }

                _context.tbl_customer.Add(customer);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login");
            }

            return View(customer);
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string customerEmail, string customerPassword)
        {
            if (string.IsNullOrEmpty(customerEmail) || string.IsNullOrEmpty(customerPassword))
            {
                ModelState.AddModelError(string.Empty, "Email and Password are required.");
                return View();
            }

            var customer = await _context.tbl_customer
                .FirstOrDefaultAsync(c => c.customer_email == customerEmail && c.customer_password == customerPassword);

            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View();
            }

            HttpContext.Session.SetString("customer_session", customer.customer_id.ToString());
            HttpContext.Session.SetString("UserIs", "Customer");
            return RedirectToAction("Index");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin_session");
            HttpContext.Session.Remove("UserIs");
            return RedirectToAction("Login","Customer");
        }
        public IActionResult Index(int? categoryId)
        {
            var products = categoryId.HasValue
                ? _context.tbl_product.Where(p => p.cat_id == categoryId.Value).ToList()
                : _context.tbl_product.ToList();

            var categories = _context.tbl_category.ToList();

            ViewBag.Categories = categories;
            ViewBag.Title = "DashBorad";
            return View(products);
        }

        public IActionResult Profile()
        {
            var customerIdStr = HttpContext.Session.GetString("customer_session");

            if (string.IsNullOrEmpty(customerIdStr))
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(customerIdStr, out int customerId))
            {
                return RedirectToAction("Error");
            }

            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == customerId);

            if (customer == null)
            {
                return RedirectToAction("NotFound");
            }

            return View(customer);
        }
        public IActionResult ViewCart()
        {
            var customerIdStr = HttpContext.Session.GetString("customer_session");

            if (string.IsNullOrEmpty(customerIdStr))
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(customerIdStr, out int customerId))
            {
                return RedirectToAction("Error");
            }

            var cartItems = _context.tbl_cart
                .Where(c => c.cust_id == customerId && c.cart_status == 1)
                .ToList();

            if (!cartItems.Any())
            {
                ViewBag.ProductDictionary = new Dictionary<int, Product>();
                return View(new List<Cart>());
            }

            var productIds = cartItems.Select(c => c.prod_id).Distinct().ToList();
            var products = _context.tbl_product
                .Where(p => productIds.Contains(p.product_id))
                .ToDictionary(p => p.product_id, p => p);

            ViewBag.ProductDictionary = products;

            return View(cartItems);
        }


        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            var customerIdStr = HttpContext.Session.GetString("customer_session");

            if (string.IsNullOrEmpty(customerIdStr) || productId <= 0)
            {
                return RedirectToAction("Error");
            }

            if (!int.TryParse(customerIdStr, out int customerId))
            {
                return RedirectToAction("Error");
            }

            var product = _context.tbl_product.Find(productId);

            if (product == null)
            {
                return RedirectToAction("Error");
            }

            var existingCartItem = _context.tbl_cart
                .FirstOrDefault(c => c.prod_id == productId && c.cust_id == customerId && c.cart_status == 1);

            if (existingCartItem != null)
            {
                existingCartItem.product_quantity++;
                _context.tbl_cart.Update(existingCartItem);
            }
            else
            {
                var cartItem = new Cart
                {
                    prod_id = productId,
                    cust_id = customerId,
                    product_quantity = 1,
                    cart_status = 1
                };
                _context.tbl_cart.Add(cartItem);
            }

            _context.SaveChanges();

            return RedirectToAction("ViewCart");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int cartId)
        {
            var cartItem = _context.tbl_cart.Find(cartId);
            if (cartItem != null)
            {
                _context.tbl_cart.Remove(cartItem);
                _context.SaveChanges();
            }

            return RedirectToAction("ViewCart");
        }

        public IActionResult Orders(bool buyNow = false)
        {
            var customerIdStr = HttpContext.Session.GetString("customer_session");

            if (string.IsNullOrEmpty(customerIdStr))
            {
                return RedirectToAction("Login");
            }

            if (!int.TryParse(customerIdStr, out int customerId))
            {
                return RedirectToAction("Error");
            }

            var orders = (from order in _context.Orders
                          join product in _context.tbl_product on order.prod_id equals product.product_id
                          where order.cust_id == customerId
                          select new
                          {
                              Order = order,
                              ProductName = product.product_name,
                              ProductImage = product.product_image
                          }).ToList();

            if (buyNow)
            {
                TempData["OrderPlacedMessage"] = "Order confirmed successfully!";
            }

            return View(orders);
        }




        public IActionResult Error()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> BuyNow(int cartId)
        {
            var cartItem = _context.tbl_cart.FirstOrDefault(c => c.cart_id == cartId);

            if (cartItem == null)
            {
                return NotFound();
            }

            var product = _context.tbl_product.FirstOrDefault(p => p.product_id == cartItem.prod_id);

            if (product == null)
            {
                return NotFound();
            }

            if (!decimal.TryParse(product.product_price, out decimal productPrice))
            {
                return BadRequest("Invalid product price.");
            }
            var customerIdStr = HttpContext.Session.GetString("customer_session");

            if (string.IsNullOrEmpty(customerIdStr) || !int.TryParse(customerIdStr, out int customerId))
            {
                return RedirectToAction("Login");
            }

            var order = new Order
            {
                prod_id = cartItem.prod_id,
                cust_id = customerId,
                quantity = cartItem.product_quantity,
                total_price = productPrice * cartItem.product_quantity,
                order_date = DateTime.Now,
                order_status = "Confirmed"
            };

            _context.Orders.Add(order);

            _context.tbl_cart.Remove(cartItem);

            await _context.SaveChangesAsync();

            TempData["OrderPlacedMessage"] = "Order placed successfully!";

            return RedirectToAction("Orders");
        }







    }
}
