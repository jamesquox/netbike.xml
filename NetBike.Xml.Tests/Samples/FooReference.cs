using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetBike.Xml.Tests.Samples
{
    public class FooReference
    {
        public IFoo Foo { get; set; }
        public IList<IFoo> Foos { get; set; }
    }
}
