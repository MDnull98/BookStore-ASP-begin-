using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Domain.Abstract;
using Domain.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using WebUI.Controllers;
using WebUI.Models;

namespace UnitTests
{
    [TestClass]
    public class UnitTest2
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            //Организация
            Book book1 = new Book { BookId = 1, Name = "Book1" };
            Book book2 = new Book { BookId = 2, Name = "Book2" };
            Cart cart = new Cart();
            //Действие
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            List<CartLine> results = cart.Lines.ToList();
            // Утверждение
            Assert.AreEqual(results.Count(), 2);
            Assert.AreEqual(results[0].Book, book1);
            Assert.AreEqual(results[1].Book, book2);

        }
        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            //Организация
            Book book1 = new Book { BookId = 1, Name = "Book1" };
            Book book2 = new Book { BookId = 2, Name = "Book2" };
            Cart cart = new Cart();
            //Действие
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            cart.AddItem(book1, 5);
            List<CartLine> results = cart.Lines.OrderBy(c => c.Book.BookId).ToList();
            // Утверждение
            Assert.AreEqual(results.Count(), 2);
            Assert.AreEqual(results[0].Quantity, 6);
            Assert.AreEqual(results[1].Quantity, 1);

        }
        [TestMethod]
        public void Can_Remove_Line()
        {
            //Организация
            Book book1 = new Book { BookId = 1, Name = "Book1" };
            Book book2 = new Book { BookId = 2, Name = "Book2" };
            Book book3 = new Book { BookId = 3, Name = "Book3" };
            Cart cart = new Cart();
            //Действие
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            cart.AddItem(book1, 5);
            cart.AddItem(book3, 2);
            cart.RemoveLine(book2);
            Assert.AreEqual(cart.Lines.Where(c => c.Book == book2).Count(), 0);
            Assert.AreEqual(cart.Lines.Count(), 2);
        }
        [TestMethod]
        public void Calculate_Cart_Total()
        {
            //Организация
            Book book1 = new Book { BookId = 1, Name = "Book1", Price = 100 };
            Book book2 = new Book { BookId = 2, Name = "Book2", Price = 55  };
            Cart cart = new Cart();
            //Действие
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            cart.AddItem(book1, 5);
            decimal result = cart.ComputeTotalValue();
            Assert.AreEqual(result, 655);
        }
        [TestMethod]
        public void Can_Clear_Contents()
        {
            //Организация
            Book book1 = new Book { BookId = 1, Name = "Book1", Price = 100 };
            Book book2 = new Book { BookId = 2, Name = "Book2", Price = 55 };
            Cart cart = new Cart();
            //Действие
            cart.AddItem(book1, 1);
            cart.AddItem(book2, 1);
            cart.AddItem(book1, 5);
            cart.Clear();
            Assert.AreEqual(cart.Lines.Count(),0);
        }
        [TestMethod]
        public void Can_Add_To_Cart()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>{
                new Book {BookId=1,Name="Book1",Genre="Genre1"}
            }.AsQueryable());
            Cart cart = new Cart();
            CartController controller = new CartController(mock.Object,null);
            controller.AddToCart(cart, 1, null);
            Assert.AreEqual(cart.Lines.Count(), 1);
            Assert.AreEqual(cart.Lines.ToList()[0].Book.BookId, 1); 

        }
        [TestMethod]
        public void Adding_Book_To_Cart_Goes_To_Cart_Screen()
        {
            Mock<IBookRepository> mock = new Mock<IBookRepository>();
            mock.Setup(m => m.Books).Returns(new List<Book>{
                new Book {BookId=1,Name="Book1",Genre="Genre1"}
            }.AsQueryable());
            Cart cart = new Cart();
            CartController controller = new CartController(mock.Object,null);
            RedirectToRouteResult result = controller.AddToCart(cart, 2, "myUrl");
            Assert.AreEqual(result.RouteValues["action"],"Index");
            Assert.AreEqual(result.RouteValues["returnUrl"],"myUrl");

        }
        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            Cart cart = new Cart();
            CartController target = new CartController(null,null);
            CartIndexViewModel result = (CartIndexViewModel)target.Index(cart, "myUrl").ViewData.Model;
            Assert.AreSame(result.Cart, cart);
            Assert.AreEqual(result.ReturnUrl, "myUrl");
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            ShippingDetails shippingDetails = new ShippingDetails();
            CartController controller = new CartController(null, mock.Object);
            ViewResult result = controller.Checkout(cart, shippingDetails);
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());
            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }
        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            cart.AddItem(new Book(), 1);

            CartController controller = new CartController(null, mock.Object);
            controller.ModelState.AddModelError("error", "error");
            ViewResult result = controller.Checkout(cart, new ShippingDetails());
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());
            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }
        [TestMethod]
        public void Cannot_Checkout_Andd_Submit_Order()
        {
            Mock<IOrderProcessor> mock = new Mock<IOrderProcessor>();
            Cart cart = new Cart();
            cart.AddItem(new Book(), 1);

            CartController controller = new CartController(null, mock.Object);
            ViewResult result = controller.Checkout(cart, new ShippingDetails());
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());
            Assert.AreEqual("Completed", result.ViewName);
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
        }
    }
}

