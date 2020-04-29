namespace NetBike.Xml.Contracts.Builders
{
    using System;

    public class XmlKnownTypeBuilder : XmlMemberBuilder, IXmlObjectBuilder
    {
        public XmlKnownTypeBuilder(Type valueType)
            : base(valueType)
        {
        }

        public XmlNullValueHandling? NullValueHandling { get; set; }

        public XmlDefaultValueHandling? DefaultValueHandling { get; set; }

        public object DefaultValue { get; set; }

        public static XmlKnownTypeBuilder Create(XmlKnownType knownType)
        {
            if (knownType == null)
            {
                throw new ArgumentNullException("knownType");
            }

            return new XmlKnownTypeBuilder(knownType.ValueType)
            {
                Name = knownType.Name,
                TypeHandling = knownType.TypeHandling,
                NullValueHandling = knownType.NullValueHandling,
                DefaultValueHandling = knownType.DefaultValueHandling,
                DefaultValue = knownType.DefaultValue
            };
        }

        public XmlKnownType Build()
        {
            return new XmlKnownType(
                ValueType,
                Name ?? ValueType.GetShortName(),
                TypeHandling,
                NullValueHandling,
                DefaultValueHandling,
                DefaultValue);
        }
    }
}