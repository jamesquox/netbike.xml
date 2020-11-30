namespace NetBike.Xml.Tests.Converters.Basics
{
    using System.Collections.Generic;
    using NetBike.Xml.Converters;
    using NetBike.Xml.Converters.Basics;
    using NUnit.Framework;

    [TestFixture]
    public class XmlStringConverterTests : XmlBasicConverterBaseTests<string>
    {
        protected override IXmlConverter GetConverter()
        {
            return new XmlStringConverter();
        }

        protected static IEnumerable<BasicSample> Samples =>
            new[]{
                new BasicSample("string", "string")
            };
    }
}