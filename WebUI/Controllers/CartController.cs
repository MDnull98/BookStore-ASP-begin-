using Domain.Abstract;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebUI.Models;

namespace WebUI.Controllers
{
    public class CartController : Controller
    {
        private IBookRepository repository;
        private IOrderProcessor orderProcessor;
        public CartController(IBookRepository repo,IOrderProcessor processor)
        {
            repository = repo;
            orderProcessor = processor;
        }
        public ViewResult Index(Cart cart,string returnUrl)
        {
            return View(new CartIndexViewModel
            {
                Cart = cart,
                ReturnUrl = returnUrl
            });
        }
        //public Cart GetCart()
        //{
        //    Cart cart = (Cart)Session["Cart"];
        //    if(cart==null)
        //    {
        //        cart = new Cart();
        //        Session["Cart"] = cart;
        //    }
        //    return cart;
        //}
        public RedirectToRouteResult AddToCart(Cart cart, int bookId,string returnUrl)
        {
                Book book = repository.Books
                .FirstOrDefault(b => b.BookId == bookId);
                if(book!=null)
                {
                   cart.AddItem(book, 1);
                }
            return RedirectToAction("Index", new { returnUrl });
        }
        public RedirectToRouteResult RemoveFromCart(Cart cart, int bookId, string returnUrl)
        {
            Book book = repository.Books
            .FirstOrDefault(b => b.BookId == bookId);
            if (book != null)
            {
                cart.RemoveLine(book);
            }
            return RedirectToAction("Index", new { returnUrl });
        }
        public PartialViewResult Summary(Cart cart)
        {
            return PartialView(cart);
        }
        public ViewResult Checkout()
        {
            return View(new ShippingDetails());
        }
        [HttpPost]
        public ViewResult Checkout(Cart cart, ShippingDetails shippingDetails)
        {
           if (cart.Lines.Count()==0)
            {
                ModelState.AddModelError("", "Извините, корзина пуста!");
            }
           if(ModelState.IsValid)
            {
                orderProcessor.ProcessOrder(cart, shippingDetails);
                cart.Clear();
                return View("Completed");
            }
           else
            {
                return View(new ShippingDetails());
            }
        }
    }
}