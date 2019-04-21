using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Essentials.Tests
{
    [TestFixture()]
    public class UtilTest
    {
        [Test()]
        public void TestSwapInts()
        {
            int a = 1;
            int b = 2;
            Util.Swap(ref a, ref b);
            Assert.AreEqual(2, a);
            Assert.AreEqual(1, b);
        }

        [Test()]
        public void TestSwapStrings()
        {
            string a = "foo";
            string b = "bar";
            Util.Swap(ref a, ref b);
            Assert.AreEqual("bar", a);
            Assert.AreEqual("foo", b);
        }

        [Test()]
        public void TestSwapDisposable()
        {
            List<string> trace = new List<string>();
            X a = new X("a", trace);
            X b = null;
            try
            {
                b = new X("b", trace);
                Util.Swap(ref a, ref b);
            }
            finally
            {
                b?.Dispose();
            }
            Assert.AreEqual("b", a.Name);
            Assert.AreEqual(new List<string> { "a" }, trace);
        }

        [Test()]
        public void TestSwapDisposableError()
        {
            List<string> trace = new List<string>();
            X a = new X("a", trace);
            Assert.Throws<ApplicationException>(() =>
            {
                X b = null;
                try
                {
                    b = new X("b", trace);
                    b.MethodThrows();
                    Util.Swap(ref a, ref b);
                }
                finally
                {
                    b?.Dispose();
                }
            });
            Assert.AreEqual("a", a.Name);
            Assert.AreEqual(new List<string> { "b" }, trace);
        }

        class X : IDisposable
        {
            List<string> _trace;
            public string Name { get; private set;  }
            public X(string name, List<string> trace)
            {
                Name = name;
                _trace = trace;
            }
            public void MethodThrows()
            {
                throw new ApplicationException("testing");
            }
            public void Dispose()
            {
                _trace.Add(Name);
            }
        }
    }
}
