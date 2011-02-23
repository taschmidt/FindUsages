using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FindUsage.Test
{
    public class Test
    {
        public void TestFunction()
        {
            FooProp.BarProp = "one";
            FooField.BarField = "two";
        }

        public bool TestFunction2(Foo foo)
        {
            return true;
        }

        public Foo GetFoo()
        {
            return null;
        }

        public void TestFunction3(IEnumerable<Foo> foos)
        {
        }

        public void TestFunction4(Foo[] foos)
        {
        }

        public Foo FooProp { get; set; }
        public Foo FooField;

        public string TestGetter
        {
            get { return FooProp.BarField; }
        }
    }

    public class Foo
    {
        public string BarProp { get; set; }
        public string BarField;
    }
}
