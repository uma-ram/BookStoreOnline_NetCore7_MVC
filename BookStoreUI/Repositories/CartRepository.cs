using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Security.Cryptography.Xml;

namespace BookStoreUI.Repositories
{
    public class CartRepository:ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;  //To get claimprincipal of the logged in user

        public CartRepository(ApplicationDbContext db,UserManager<IdentityUser> userManager,IHttpContextAccessor httpContextAccessor)
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
                    var book = _db.Books.Find(bookID); 
                    cartItem = new CartDetail
                    {
                        BookId = bookID,
                        Quantity = qty,
                        ShoppingCartId = cart.Id,
                        UnitPrice = book.Price
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
            if ((string.IsNullOrEmpty(userID)))
            {
                userID = GetUserID();
            }

            var cartItems = await (from cart in _db.ShoppingCarts
                                   join cartDetail in _db.CartDetails
                                   on cart.Id equals cartDetail.ShoppingCartId
                                   where cart.UserId == userID
                                   select new { cartDetail.Id }
                                  ).ToListAsync();
            return cartItems.Count;
        }
        public async  Task<bool> DoCheckOut()
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                //Logic->
                //Move records from cartdetails to order and order details and then delete from cart details.

                var userID = GetUserID();
                if (string.IsNullOrEmpty(userID))
                    throw new Exception("User Not Logged In");
                var cart =await GetCart(userID);
                if (cart is null)
                    throw new Exception("Invalid Cart");
                var cartDetails = _db.CartDetails
                                    .Where(a => a.ShoppingCartId == cart.Id).ToList();
                if (cartDetails.Count() == 0)
                    throw new Exception("Cart is empty");

                var order = new Order
                {
                    UserId = userID,
                    OrderStatusId = 1, // pending
                    CreateDate = DateTime.UtcNow
                };
                _db.Orders.Add(order);
                _db.SaveChanges();

                foreach(var item in cartDetails)
                {
                    var orderDetails = new OrderDetail
                    {
                        OrderId = order.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        BookId = item.BookId
                    };
                    _db.OrderDetails.Add(orderDetails);
                }
                _db.SaveChanges();

                //remove cartdetails 
                _db.CartDetails.RemoveRange(cartDetails);
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }

        }
        
        //Get userid from Claim principal - private method=>can be public in separete file

        private string GetUserID()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userID = _userManager.GetUserId(principal);
            return userID;
        }




    }
}
