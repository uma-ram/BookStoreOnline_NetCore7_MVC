using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStoreUI.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;

        public CartController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }
        //Add new item to cart
        public async Task<IActionResult> AddItem(int bookID, int qty=1, int redirect=0)
        {
            var cartCount = await _cartRepository.AddToCart(bookID, qty);
            if (redirect == 0)
                return Ok(cartCount);
            return RedirectToAction("GetUserCart");
        }

        //Remove an item to cart
        public async Task<IActionResult> RemoveItem(int bookID)
        {
            var cartCount = await _cartRepository.DeleteFromCart(bookID);
            return RedirectToAction("GetUserCart");
        }

        public async Task<IActionResult> GetUserCart()
        {
            var cart =await _cartRepository.GetUserCart();
            return View(cart);
        }

        public async Task<IActionResult> GetTotalItemsinCart()
        {
            var itemCount = await _cartRepository.GetCartItemCount();
            return Ok(itemCount);
        }

        public async Task<IActionResult> CheckOut()
        {
            var isCheckOut = await _cartRepository.DoCheckOut();
            if(!isCheckOut)
                throw new Exception("Something happen in server side");
            return RedirectToAction("Index","Home");
        }
    }
}
