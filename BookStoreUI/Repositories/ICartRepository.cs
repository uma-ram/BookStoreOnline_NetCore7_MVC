namespace BookStoreUI.Repositories
{
    public interface ICartRepository
    {
        Task<int> AddToCart(int bookID, int qty);
        Task<int> DeleteFromCart(int bookID);

        Task<ShoppingCart> GetUserCart();

        Task<ShoppingCart> GetCart(string userID);
        Task<int> GetCartItemCount(string userID = "");

        Task<bool> DoCheckOut();
    }
}