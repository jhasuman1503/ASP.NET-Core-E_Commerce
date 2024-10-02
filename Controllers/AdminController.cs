using E_Commerce_Website.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace E_Commerce_Website.Controllers
{
    public class AdminController : Controller
    {
        private readonly myContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminController(myContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var adminSession = HttpContext.Session.GetString("admin_session");
            if (adminSession != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string adminEmail, string adminPassword)
        {
            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                ViewBag.message = "Email and Password are required.";
                return View();
            }

            var admin = await _context.tbl_admin
                .FirstOrDefaultAsync(a => a.admin_email == adminEmail && a.admin_password == adminPassword);

            if (admin != null)
            {
                HttpContext.Session.SetString("UserIs", "Admin");
                HttpContext.Session.SetString("admin_session", admin.admin_id.ToString());
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.message = "Incorrect username or password.";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("admin_session");
            HttpContext.Session.Remove("UserIs");
            return RedirectToAction("Login");
        }

        public IActionResult Profile()
        {
            try
            {
                var adminIdStr = HttpContext.Session.GetString("admin_session");

                if (string.IsNullOrEmpty(adminIdStr))
                {
                    return RedirectToAction("Login");
                }

                if (!int.TryParse(adminIdStr, out int adminId))
                {
                    return RedirectToAction("Error");
                }

                var admin = _context.tbl_admin.FirstOrDefault(a => a.admin_id == adminId);

                if (admin == null)
                {
                    return RedirectToAction("NotFound");
                }

                return View(admin);
            }
            catch
            {
                return RedirectToAction("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Profile(Admin admin, IFormFile admin_image)
        {
            if (ModelState.IsValid)
            {
                var existingAdmin = await _context.tbl_admin.FindAsync(admin.admin_id);

                if (existingAdmin != null)
                {
                    existingAdmin.admin_name = admin.admin_name;
                    existingAdmin.admin_email = admin.admin_email;
                    existingAdmin.admin_password = admin.admin_password;

                  
                    if (admin_image != null && admin_image.Length > 0)
                    {
                        string imagePath = Path.Combine(_env.WebRootPath, "admin_images", admin_image.FileName);
                        using (var fs = new FileStream(imagePath, FileMode.Create))
                        {
                            await admin_image.CopyToAsync(fs);
                        }
                        existingAdmin.admin_image = admin_image.FileName;
                    }

                    _context.tbl_admin.Update(existingAdmin);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Profile");
                }
            }

            return View(admin);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeProfileImage(IFormFile admin_image, int admin_id)
        {
            if (admin_image != null && admin_image.Length > 0)
            {
                var existingAdmin = await _context.tbl_admin.FindAsync(admin_id);

                if (existingAdmin != null)
                {
                    string imagePath = Path.Combine(_env.WebRootPath, "admin_images", admin_image.FileName);
                    using (var fs = new FileStream(imagePath, FileMode.Create))
                    {
                        await admin_image.CopyToAsync(fs);
                    }
                    existingAdmin.admin_image = admin_image.FileName;
                    _context.tbl_admin.Update(existingAdmin);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Profile");
        }

        public IActionResult FetchCustomer()
        {
            return View(_context.tbl_customer.ToList());
        }

        public IActionResult CustomerDetails(int id)
        {
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == id);
            return customer != null ? View(customer) : RedirectToAction("NotFound");
        }

        public IActionResult UpdateCustomer(int id)
        {
            var customer = _context.tbl_customer.Find(id);
            return customer != null ? View(customer) : RedirectToAction("NotFound");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCustomer(Customer customer, IFormFile customer_image)
        {
            if (customer_image != null && customer_image.Length > 0)
            {
                string imagePath = Path.Combine(_env.WebRootPath, "customer_images", customer_image.FileName);
                using (var fs = new FileStream(imagePath, FileMode.Create))
                {
                    await customer_image.CopyToAsync(fs);
                }
                customer.customer_image = customer_image.FileName;
            }
            else
            {
                customer.customer_image = "";
            }
            _context.tbl_customer.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction("FetchCustomer");
        }

        public IActionResult DeletePermission(int id)
        {
            var customer = _context.tbl_customer.FirstOrDefault(c => c.customer_id == id);
            return customer != null ? View(customer) : RedirectToAction("NotFound");
        }

        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.tbl_customer.FindAsync(id);
            if (customer != null)
            {
                _context.tbl_customer.Remove(customer);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("FetchCustomer");
        }

        public IActionResult FetchCategory()
        {
            return View(_context.tbl_category.ToList());
        }

        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(Category cat)
        {
            if (ModelState.IsValid)
            {
                _context.tbl_category.Add(cat);
                await _context.SaveChangesAsync();
                return RedirectToAction("FetchCategory");
            }
            return View(cat);
        }

        public IActionResult UpdateCategory(int id)
        {
            var category = _context.tbl_category.Find(id);
            return category != null ? View(category) : RedirectToAction("NotFound");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(Category cat)
        {
            if (ModelState.IsValid)
            {
                _context.tbl_category.Update(cat);
                await _context.SaveChangesAsync();
                return RedirectToAction("FetchCategory");
            }
            return View(cat);
        }

        public IActionResult DeletePermissionCategory(int id)
        {
            var category = _context.tbl_category.FirstOrDefault(c => c.category_id == id);
            return category != null ? View(category) : RedirectToAction("NotFound");
        }

        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.tbl_category.FindAsync(id);
            if (category != null)
            {
                _context.tbl_category.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("FetchCategory");
        }

        public IActionResult FetchProduct()
        {
            var products = _context.tbl_product.ToList();
            return View(products); // Ensure you have a corresponding view for this action
        }


        public IActionResult AddProduct()
        {
            ViewData["Category"] = _context.tbl_category.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product prod, IFormFile product_image)
        {
            if (product_image != null && product_image.Length > 0)
            {
                string imageName = Path.GetFileName(product_image.FileName);
                string imagePath = Path.Combine(_env.WebRootPath, "product_images", imageName);
                using (var fs = new FileStream(imagePath, FileMode.Create))
                {
                    await product_image.CopyToAsync(fs);
                }
                prod.product_image = imageName;
            }
            _context.tbl_product.Add(prod);
            await _context.SaveChangesAsync();
            return RedirectToAction("FetchProduct");
        }

        public IActionResult ProductDetails(int id)
        {
            var product = _context.tbl_product.Include(p => p.Category).FirstOrDefault(p => p.product_id == id);
            return product != null ? View(product) : RedirectToAction("NotFound");
        }

        public IActionResult DeletePermissionProduct(int id)
        {
            var product = _context.tbl_product.FirstOrDefault(p => p.product_id == id);
            return product != null ? View(product) : RedirectToAction("NotFound");
        }

        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.tbl_product.FindAsync(id);
            if (product != null)
            {
                _context.tbl_product.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("FetchProduct");
        }

        public IActionResult UpdateProduct(int id)
        {
            ViewData["Category"] = _context.tbl_category.ToList();
            var product = _context.tbl_product.Find(id);
            return product != null ? View(product) : RedirectToAction("NotFound");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(Product prod, IFormFile product_image)
        {
            if (ModelState.IsValid)
            {
                var existingProduct = await _context.tbl_product.FindAsync(prod.product_id);
                if (existingProduct != null)
                {
                    existingProduct.product_name = prod.product_name;
                    existingProduct.product_description = prod.product_description;
                    existingProduct.product_price = prod.product_price;

                    if (product_image != null && product_image.Length > 0)
                    {
                        string imageName = Path.GetFileName(product_image.FileName);
                        string imagePath = Path.Combine(_env.WebRootPath, "product_images", imageName);
                        using (var fs = new FileStream(imagePath, FileMode.Create))
                        {
                            await product_image.CopyToAsync(fs);
                        }
                        existingProduct.product_image = imageName;
                    }

                    _context.tbl_product.Update(existingProduct);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("FetchProduct"); // Ensure you have a FetchProduct action
                }
            }
            return View(prod);
        }



        public IActionResult Error()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View();
        }
        public IActionResult Home(int? categoryId)
        {
            var categories = _context.tbl_category.ToList();

            var products = _context.tbl_product.AsQueryable();

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.cat_id == categoryId.Value);
            }

            var viewModel = new ProductCategoryViewModel
            {
                Categories = categories,
                Products = products.ToList()
            };

            return View(viewModel);
        }
        [HttpPost]
        public IActionResult AddToCart(int productId, int customerId)
        {
            if (productId <= 0 || customerId <= 0)
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
        public IActionResult ViewCart()
        {
            int customerId = 1;

            var cartItems = _context.tbl_cart
                .Where(c => c.cust_id == customerId && c.cart_status == 1)
                .ToList();

            Console.WriteLine($"Cart Items Count: {cartItems.Count}");

            if (!cartItems.Any())
            {
                ViewBag.ProductDictionary = new Dictionary<int, Product>();
                return View(new List<Cart>());
            }

            var productIds = cartItems.Select(c => c.prod_id).Distinct().ToList();

            Console.WriteLine("Product IDs: " + string.Join(", ", productIds));

            var products = _context.tbl_product
                .Where(p => productIds.Contains(p.product_id))
                .ToDictionary(p => p.product_id, p => p);

            Console.WriteLine($"Products Count: {products.Count}");

            ViewBag.ProductDictionary = products;

            return View(cartItems);
        }

        [HttpPost]
        public IActionResult OrderNow(int productId, int quantity)
        {
            var cartItem = _context.tbl_cart
                .Where(c => c.prod_id == productId && c.product_quantity == quantity && c.cart_status == 1)
                .FirstOrDefault();

            if (cartItem != null)
            {
               
            }

           
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
        [HttpPost]
        public IActionResult BuyNow(int cartId)
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

            var order = new Order
            {
                prod_id = cartItem.prod_id,
                cust_id = cartItem.cust_id,
                quantity = cartItem.product_quantity,
                total_price = productPrice * cartItem.product_quantity,
                order_date = DateTime.Now,
                order_status = "Confirmed"
            };

            _context.Orders.Add(order);
            _context.tbl_cart.Remove(cartItem);
            _context.SaveChanges();

            return RedirectToAction("OrderSuccess");
        }





        public IActionResult OrderSuccess()
        {
            return View();
        }
        public IActionResult Orders()
        {
            var orders = _context.Orders.ToList();
            return View(orders);
        }

    }
}
