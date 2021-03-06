﻿namespace NetBike.Xml.Tests.Converters.Basics
{
    using System.Collections.Generic;
    using Contracts;
    using NetBike.Xml.Converters;
    using NetBike.Xml.Converters.Basics;
    using NUnit.Framework;

    [TestFixture]
    public class XmlInt32ConverterTests : XmlBasicConverterBaseTests<int>
    {
        protected override IXmlConverter GetConverter()
        {
            return new XmlInt32Converter();
        }

        protected static IEnumerable<BasicSample> Samples => 
            new[] { new BasicSample("123", 123) };
    }
}