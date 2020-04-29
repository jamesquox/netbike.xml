﻿namespace NetBike.Xml.Contracts
{
    using System;
    using System.Collections.Generic;
    using Builders;
    using Utilities;

    public sealed partial class XmlObjectContract : XmlContract
    {
        private static readonly List<XmlProperty> EmptyProperties = new List<XmlProperty>();

        private readonly XmlTypeHandling? typeHandling;
        private readonly XmlItem item;
        private readonly List<XmlProperty> properties;
        private readonly bool hasRequiredOrDefaultsProperties;
        private readonly XmlProperty innerTextProperty;

        public XmlObjectContract(
            Type valueType,
            XmlName name,
            IEnumerable<XmlProperty> properties = null,
            XmlTypeHandling? typeHandling = null,
            XmlItem item = null)
            : base(valueType, name)
        {
            var elementCount = 0;

            if (properties == null)
            {
                properties = EmptyProperties;
            }

            this.item = item;
            this.typeHandling = typeHandling;
            this.properties = new List<XmlProperty>(properties);

            foreach (var property in this.properties)
            {
                if (property == null)
                {
                    throw new ArgumentNullException("properties.property");
                }

                if (valueType != property.PropertyInfo.ReflectedType)
                {
                    throw new ArgumentException("Property must be declared in contract type.", "properties.property");
                }

                if (property.IsRequired || property.DefaultValue != null)
                {
                    hasRequiredOrDefaultsProperties = true;
                }

                if (property.MappingType == XmlMappingType.Element)
                {
                    elementCount++;
                }
                else if (property.MappingType == XmlMappingType.InnerText)
                {
                    if (innerTextProperty != null)
                    {
                        throw new XmlSerializationException("Contract must have only one innerText property.");
                    }

                    innerTextProperty = property;
                }
            }

            if (innerTextProperty != null && elementCount > 0)
            {
                throw new XmlSerializationException("Contract must not contain elements, if it contains innerText property.");
            }

            this.properties.Sort(XmlPropertyComparer.Instance);
        }

        public IReadOnlyList<XmlProperty> Properties
        {
            get { return properties; }
        }

        public XmlItem Item
        {
            get { return item; }
        }

        public XmlTypeHandling? TypeHandling
        {
            get { return typeHandling; }
        }

        internal bool HasRequiredOrDefaultsProperties
        {
            get { return hasRequiredOrDefaultsProperties; }
        }

        internal XmlProperty InnerTextProperty
        {
            get { return innerTextProperty; }
        }
        
        protected override XmlMember GetDefaultMember()
        {
            return new XmlMember(
                ValueType,
                Name,
                XmlMappingType.Element,
                typeHandling,
                null,
                null,
                null,
                item);
        }
    }
}