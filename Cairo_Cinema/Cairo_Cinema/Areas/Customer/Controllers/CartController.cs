using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

namespace Cairo_Cinema.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IRepository<Cart> _cartRepository;
        private readonly IRepository<Movie> _movieRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRepository<Coupon> _couponRepository;
        public CartController(IRepository<Cart> cartRepository, IRepository<Movie> movieRepository, UserManager<ApplicationUser> userManager, IRepository<Coupon> couponRepository)
        {
            _cartRepository = cartRepository;
            _movieRepository = movieRepository;
            _userManager = userManager;
            _couponRepository = couponRepository;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var carts = await _cartRepository.GetListAsync(m => m.ApplicationUserId == user.Id, includes: [m => m.Movie]);
            var subtotal = carts.Sum(c => c.Movie.Price * c.Count);
            decimal discount = 0;
            var code = HttpContext.Session.GetString("CouponCode");
            if (code != null)
            {
                var coupon = await _couponRepository.GetOneAsync(c =>
                    c.Code == code && c.IsActive && c.ExpiryDate >= DateTime.UtcNow);

                if (coupon != null)
                    discount = subtotal * coupon.DiscountPercent / 100;
                else
                    HttpContext.Session.Remove("CouponCode");   // الكود انتهى أو تعطّل
            }

            ViewBag.Subtotal = subtotal;
            ViewBag.Discount = discount;
            ViewBag.Total = subtotal - discount;
            ViewBag.CouponCode = HttpContext.Session.GetString("CouponCode");

            return View(carts);
        }
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string code)
        {
            code = code?.Trim() ?? "";

            var coupon = await _couponRepository.GetOneAsync(c =>
                c.Code == code && c.IsActive && c.ExpiryDate >= DateTime.UtcNow);

            if (coupon is null)
            {
                TempData["CouponError"] = "Invalid or expired code.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.SetString("CouponCode", coupon.Code);
            return RedirectToAction(nameof(Index));
        }
        public IActionResult RemoveCoupon()
        {
            HttpContext.Session.Remove("CouponCode");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> AddToCart(int movieId, int count)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var movie = await _movieRepository.GetOneAsync(m => m.Id == movieId);
            if (movie is null) return NotFound();
            if (movie.Quantity < count) return BadRequest("Invalid quantity.");
            var existingMovie = await _cartRepository.GetOneAsync(c =>
            c.ApplicationUserId == user.Id &&
            c.MovieId == movieId);
            if (existingMovie != null)
            {
                var x = existingMovie.Count + count;
                if (movie.Quantity < x) return BadRequest("Invalid quantity.");
                existingMovie.Count += count;
                await _cartRepository.CommitAsync();
                return RedirectToAction(nameof(Index));   // ← أضف هذا السطر
            }
            var cart = new Cart()
            {
                ApplicationUserId = user.Id,
                MovieId = movieId,
                Count = count
            };
            await _cartRepository.AddAsync(cart);
            await _cartRepository.CommitAsync();

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> IncrementMovie(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var movie = await _movieRepository.GetOneAsync(m => m.Id == movieId);
            if (movie is null) return NotFound();
            var cart = await _cartRepository.GetOneAsync(c =>
            c.ApplicationUserId == user.Id &&
            c.MovieId == movieId
            );
            if (cart == null) return NotFound();
            if (movie.Quantity <= cart.Count)
            {
                TempData["Error_Validation"] = "Invalid quantity";
                return RedirectToAction(nameof(Index));
            }
            cart.Count++;
            await _cartRepository.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DecrementMovie(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var movie = await _movieRepository.GetOneAsync(m => m.Id == movieId);
            if (movie is null) return NotFound();
            var cart = await _cartRepository.GetOneAsync(c =>
            c.ApplicationUserId == user.Id &&
            c.MovieId == movieId
            );
            if (cart == null) return NotFound();
            if (cart.Count == 1)
            {
                _cartRepository.Remove(cart);
            }
            cart.Count--;
            await _cartRepository.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteMovie(int movieId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user is null) return NotFound();
            var movie = await _movieRepository.GetOneAsync(m => m.Id == movieId);
            if (movie is null) return NotFound();
            var cart = await _cartRepository.GetOneAsync(c =>
            c.ApplicationUserId == user.Id &&
            c.MovieId == movieId
            );
            if (cart == null) return NotFound();
            _cartRepository.Remove(cart);
            await _cartRepository.CommitAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Pay()
        {
            var options = new SessionCreateOptions
            {
                AllowedPaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>(),

                /*    LineItems = new List<SessionLineItemOptions>()
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = model.Currency,
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = model.ProductName,
                                Description = model.ProductDescription,
                            },
                            UnitAmount = model.Amount,
                        },
                        Quantity = 1,
                    },
                },*/
                Mode = "payment",
                SuccessUrl = $"{Request.Scheme}://{Request.Host}/checkout/success",
                CancelUrl = $"{Request.Scheme}://{Request.Host}/checkout/cancel",
            };
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            var carts = await _cartRepository.GetListAsync(c => c.ApplicationUserId == user.Id, includes: [m => m.Movie]);
            foreach (var item in carts)
            {
                options.LineItems.Add(new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "egp",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Movie.Name,
                            Description = item.Movie.Description,
                        },
                        UnitAmount = (long)(item.Movie.Price * 100), // Stripe expects amount in cents
                    },
                    Quantity = item.Count,
                });
            }
            var service = new SessionService();
            var session = service.Create(options);
            return Redirect(session.Url);
        }
    }
}
