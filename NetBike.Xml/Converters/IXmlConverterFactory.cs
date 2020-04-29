namespace NetBike.Xml.Converters
{
    using System;
    using Contracts;

    public interface IXmlConverterFactory
    {
        IXmlConverter CreateConverter(XmlContract contract);
    }
}