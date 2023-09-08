namespace BookStoreUI
{
    public interface IHomeRepository
    {
        Task<IEnumerable<Book>> GetBooks(string sTerm = "", int genreID = 0);
        Task<IEnumerable<Genre>> GetGenre();
    }
}