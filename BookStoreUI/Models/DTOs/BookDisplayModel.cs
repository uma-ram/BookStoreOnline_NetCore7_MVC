namespace BookStoreUI.Models.DTOs
{
    public class BookDisplayModel
    {
        public IEnumerable<Book> Books { get; set; }
        public IEnumerable<Genre> Genres { get; set; }

        public string STerm { get; set; } = "";

        public int GenreID { get; set; } = 0;
    }
}
