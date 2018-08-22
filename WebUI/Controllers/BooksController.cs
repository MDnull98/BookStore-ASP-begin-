using Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class BooksController : Controller
    {
        private IBookRepository repository;
        public int pageSize = 4;
        public BooksController(IBookRepository repo)
        {
            repository = repo;
        }

        public ViewResult List(string genre, int page = 1)
        {
            BooksListViewModel model = new BooksListViewModel
            {
                Books = repository.Books
            .Where(b => genre == null||b.Genre==genre)
            .OrderBy(book => book.BookId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize),
                PagingInfo = new PagingInfo
                {
                    CurrentPage = page,
                    ItemsPerPage = pageSize,
                    TotalItems = genre == null?
                    repository.Books.Count():
                    repository.Books.Where(book=> book.Genre==genre).Count()
                },
                CurrentGenre = genre
            };
            return View(model);
        }
    }
}