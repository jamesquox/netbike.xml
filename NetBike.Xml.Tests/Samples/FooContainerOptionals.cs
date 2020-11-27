using OptionalSharp;

namespace NetBike.Xml.Tests.Samples
{
    public class FooContainerOptionals
    {
        public Optional<IFoo> Foo { get; set; }
        public Optional<FooReference> ReferenceB { get; set; }
        public Optional<FooReference> ReferenceA { get; set; }
    }
}