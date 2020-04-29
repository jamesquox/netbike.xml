﻿namespace NetBike.Xml.Converters
{
    using System;
    using System.Xml;

    public abstract class XmlBasicConverter<T> : XmlConverter<T>
    {
        public override void WriteXml(XmlWriter writer, object value, XmlSerializationContext context)
        {
            var valueString = ToString((T)value, context);

            if (valueString != null)
            {
                writer.WriteString(valueString);
            }
        }

        public override object ReadXml(XmlReader reader, XmlSerializationContext context)
        {
            var value = reader.ReadAttributeOrElementContent();
            return Parse(value, context);
        }

        protected abstract T Parse(string value, XmlSerializationContext context);

        protected abstract string ToString(T value, XmlSerializationContext context);
    }
}