using BookStoreUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;

namespace BookStoreUI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeRepository _homeRepository;

        public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
        {
            _logger = logger;
            _homeRepository = homeRepository;
        }

        
        public async Task<IActionResult> Index(string sTerm="", int genreID=0)
        {
            
            IEnumerable<Book> booksList = await _homeRepository.GetBooks(sTerm,genreID);
            IEnumerable<Genre> GenreList = await _homeRepository.GetGenre();
            BookDisplayModel bookDisplayModel = new BookDisplayModel
            {
                Books = booksList,
                Genres = GenreList,
                STerm = sTerm,
                GenreID = genreID
            };
            return View(bookDisplayModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}