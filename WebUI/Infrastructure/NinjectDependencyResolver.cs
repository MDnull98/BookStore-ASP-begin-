using Domain.Abstract;
using Domain.Concrete;
using Domain.Entities;
using Moq;
using Ninject;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private IKernel kernel;
        public NinjectDependencyResolver(IKernel kernelParam)
        {
            kernel = kernelParam;
            AddBindings();
        }
        private void AddBindings()
        {
            //Mock<IBookRepository> mock = new Mock<IBookRepository>();
            //mock.Setup(m => m.Books).Returns(new List<Book>
            //{
            //    new Book { Name = "Судьба человека", Author = "М.Шолохов", Price = 11 },
            //    new Book { Name = "Война и мир", Author = "Л.Толстой", Price = 30 },
            //    new Book { Name = "Сказка о самоубийстве", Author = "А.Полярный", Price = 20 },
            //    new Book { Name = "C# и платформа .NET 4.5 для профессионалов", Author = "Карли Уотсон и др.", Price = 65 },
            //});
            kernel.Bind<IBookRepository>().To<EFBookRepository>();
            EmailSettings emailSettings = new EmailSettings
            {
                WriteAsFile = bool.Parse(ConfigurationManager.AppSettings["Email.WriteAsFile"]?? "false")
            };
             kernel.Bind<IOrderProcessor>().To<EmailOrderProcessor>()
            .WithConstructorArgument("settings", emailSettings);
        }
        
        public object GetService(Type serviceType)
        {
            return kernel.TryGet(serviceType);
        }
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return kernel.GetAll(serviceType);
        }
    }
}