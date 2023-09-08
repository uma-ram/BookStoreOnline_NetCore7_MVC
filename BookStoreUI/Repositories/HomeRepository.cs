using Microsoft.EntityFrameworkCore;

namespace BookStoreUI.Repositories
{
    public class HomeRepository:IHomeRepository
    {
        private readonly ApplicationDbContext _db;

        public HomeRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IEnumerable<Genre>> GetGenre()
        {
            return await _db.Genres.ToListAsync();

        }
        public async Task<IEnumerable<Book>> GetBooks(string sTerm="", int genreID =0)
        {
            sTerm = sTerm.ToLower();
            IEnumerable<Book> booksList = await (from book in _db.Books
                             join genre in _db.Genres
                             on book.GenreId equals genre.Id
                             where string.IsNullOrWhiteSpace(sTerm) || (book != null && book.BookName.ToLower().StartsWith(sTerm)) 
                             select new Book
                             {
                                 Id = book.Id,
                                 BookName = book.BookName,
                                 AuthorName = book.AuthorName,
                                 Price = book.Price,
                                 Image = book.Image,
                                 GenreId = book.GenreId,
                                 GenreName = genre.GenreName
                             }).ToListAsync();
            if(genreID >0)
            {
                booksList = booksList.Where(a => a.GenreId == genreID).ToList();
            }
            return booksList;
        }
    }
}
