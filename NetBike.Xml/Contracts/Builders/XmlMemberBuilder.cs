﻿namespace NetBike.Xml.Contracts.Builders
{
    using System;

    public class XmlMemberBuilder : IXmlBuilder
    {
        public XmlMemberBuilder(Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            ValueType = valueType;
        }

        public Type ValueType { get; private set; }

        public XmlName Name { get; set; }

        public XmlTypeHandling? TypeHandling { get; set; }
    }
}