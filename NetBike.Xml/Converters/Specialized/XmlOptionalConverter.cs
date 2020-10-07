namespace NetBike.Xml.Converters.Specialized
{
    using System;
    using System.Xml;
    using Contracts;
    using OptionalSharp;

    public sealed class XmlOptionalConverter : IXmlConverter
    {
        public bool CanRead(Type valueType)
        {
            return valueType.IsOptional();
        }

        public bool CanWrite(Type valueType)
        {
            return valueType.IsOptional();
        }

        public void WriteXml(XmlWriter writer, object value, XmlSerializationContext context)
        {
            var underlyingType = context.ValueType.GetUnderlyingOptionalType();
            var optional = value as IAnyOptional;

            var underlyingValue = optional.HasValue ? optional.Value : underlyingType.GetDefault();
            context.SerializeBody(writer, underlyingValue, underlyingType);
        }

        public object ReadXml(XmlReader reader, XmlSerializationContext context)
        {
            var member = context.Member;
            var underlyingType = member.ValueType.GetUnderlyingOptionalType();

            if (member.MappingType == XmlMappingType.Element)
            {
                if (!context.ReadValueType(reader, ref underlyingType))
                {
                    reader.Skip();
                    return null;
                }
            }

            return context.Deserialize(reader, underlyingType);
        }
    }
}