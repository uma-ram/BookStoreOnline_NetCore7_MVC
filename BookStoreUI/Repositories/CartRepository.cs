using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStoreUI.Repositories
{
    public class CartRepository:ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly HttpContextAccessor _httpContextAccessor;  //To get claimprincipal of the logged in user

        public CartRepository(ApplicationDbContext db,UserManager<IdentityUser> userManager,HttpContextAccessor httpContextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        //To Add item to cart

        public async Task<int> AddToCart(int bookID, int qty)
        {
            var userID = GetUserID();
            if(string.IsNullOrEmpty(userID))
            {
                throw new Exception("User not logged in");
            }
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var cart = await GetCart(userID);
                if (cart == null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userID
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();

                //cart details 
                var cartItem = _db.CartDetails.FirstOrDefault(x => x.BookId == bookID && x.ShoppingCartId == cart.Id);

                if(cartItem != null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    cartItem = new CartDetail
                    {
                        BookId = bookID,
                        Quantity = qty,
                        ShoppingCartId = cart.Id
                        //UnitPrice = price
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
            }
            catch(Exception ex)
            {
               
            }
            var cartItemCount= await GetCartItemCount(userID);
            return cartItemCount;
        }

        //To delete an item from cart
        public async Task<int> DeleteFromCart(int bookID)
        {
            var userID = GetUserID();
            if (string.IsNullOrEmpty(userID))
            {
                throw new Exception("User not logged in");
            }
            //using var transaction = _db.Database.BeginTransaction();
            try
            {
                var cart = await GetCart(userID);
                if (cart == null)
                {
                    throw new Exception("Invalid Cart");
                }
                  //cart details 
                var cartItem = _db.CartDetails.FirstOrDefault(x => x.BookId == bookID && x.ShoppingCartId == cart.Id);

                if (cartItem is null) //cart item is empty
                {
                    throw new Exception("No item in cart");
                }
                else if (cartItem.Quantity == 1)  //only one item in cart, so remove the entry from table
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else //otherwise just remove 1 qty
                {
                    cartItem.Quantity--;
                }
                    
                _db.SaveChanges();
                //transaction.Commit();
            }
            catch (Exception ex)
            {
               // return false;
            }
            var cartItemCount = await GetCartItemCount(userID);
            return cartItemCount;

        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userID = GetUserID();

            if (userID is null)
                throw new Exception("Invalid User");
            var shoppingCart = await _db.ShoppingCarts
                                .Include(a => a.CartDetails)
                                .ThenInclude(a => a.Book)
                                .ThenInclude(a => a.Genre)
                                .Where(a => a.UserId == userID).FirstOrDefaultAsync();
            return shoppingCart;
        }

        //Get Cart details for the given user ID
        public async Task<ShoppingCart> GetCart(string userID)
        {
            return await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userID);
        }

        //Get cart item count for the user

        public async Task<int> GetCartItemCount(string userID="")
        {
            if (!(string.IsNullOrEmpty(userID)))
            {
                userID = GetUserID();
            }

            var cartItems = await (from cart in _db.ShoppingCarts
                                   join cartDetail in _db.CartDetails
                                   on cart.Id equals cartDetail.ShoppingCartId
                                   select new { cartDetail.Id }
                                  ).ToListAsync();
            return cartItems.Count;
        }

        
        //Get userid from Claim principal

        private string GetUserID()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userID = _userManager.GetUserId(principal);
            return userID;
        }

    }
}
