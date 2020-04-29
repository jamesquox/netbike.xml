namespace NetBike.Xml.Tests.Samples
{
    public interface IFoo
    {
        int Id { get; set; }

        string Name { get; set; }

        FooReference Reference { get; set; }
    }
}