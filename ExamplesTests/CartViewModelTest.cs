using NUnit.Framework;
using System;
using System.Collections.Generic;
using Essentials.Examples;
using System.Threading.Tasks;

namespace Essentials.Examples.Tests
{
    [TestFixture()]
    public class CartViewModelTest
    {
        [Test()]
        public void TestCheckoutEmptiesCart()
        {
            var vm = new CartViewModel(new UnitTestErrorHandler());
            vm.Cart = new List<string> { "Wensleydale" };
            vm.CheckoutCommand.ExecuteAsync(null).Wait();
            Assert.That(vm.Cart, Is.Empty);
        }
    }
}
