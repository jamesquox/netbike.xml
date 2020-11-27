using System.Collections.Generic;

namespace NetBike.Xml.Tests.Samples
{
    public class FooReference
    {
        public IFoo Foo { get; set; }
        public IList<IFoo> Foos { get; set; }
        public FooReference Reference { get; set; }
    }
}
