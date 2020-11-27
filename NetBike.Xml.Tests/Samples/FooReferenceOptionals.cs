using OptionalSharp;
using System.Collections.Generic;

namespace NetBike.Xml.Tests.Samples
{
    public class FooReferenceOptionals
    {
        public Optional<IFoo> Foo { get; set; }
        public Optional<IList<IFoo>> Foos { get; set; }
        public Optional<FooReference> Reference { get; set; }
    }
}
