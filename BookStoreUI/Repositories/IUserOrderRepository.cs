using Microsoft.AspNetCore.Mvc;

namespace BookStoreUI.Repositories
{
    public interface IUserOrderRepository
    {
        Task<IEnumerable<Order>> UserOrders();
    }
}