using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FindUsage.Test
{
    public class Test
    {
        public void TestSetters()
        {
            FooProp.BarProp = "one";
            FooField.BarField = "two";
        }

        public bool TestParameter(Foo foo)
        {
            return true;
        }

        public Foo TestReturnValue()
        {
            return null;
        }

        public void TestGenericArgument(IEnumerable<Foo> foos)
        {
        }

        public void TestArray(Foo[] foos)
        {
        }

        public void TestObjectInitializer()
        {
            new Foo
                {
                    BarProp = "test"
                };
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
