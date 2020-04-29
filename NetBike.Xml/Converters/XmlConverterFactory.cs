namespace NetBike.Xml.Converters
{
    using System;
    using System.Xml;
    using Contracts;

    public abstract class XmlConverterFactory : IXmlConverterFactory, IXmlConverter
    {
        public virtual IXmlConverter CreateConverter(XmlContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException("contract");
            }

            var valueType = contract.ValueType;

            if (!AcceptType(valueType))
            {
                throw new ArgumentException(string.Format("Type \"{0}\" is not acceptable.", valueType), "contract");
            }

            var converterType = GetConverterType(valueType);
            return (IXmlConverter)Activator.CreateInstance(converterType);
        }

        public virtual bool CanRead(Type valueType)
        {
            return AcceptType(valueType);
        }

        public virtual bool CanWrite(Type valueType)
        {
            return AcceptType(valueType);
        }

        public void WriteXml(XmlWriter writer, object value, XmlSerializationContext context)
        {
            var converter = CreateConverter(context.Contract);
            converter.WriteXml(writer, value, context);
        }

        public object ReadXml(XmlReader reader, XmlSerializationContext context)
        {
            var converter = CreateConverter(context.Contract);
            return converter.ReadXml(reader, context);
        }

        protected abstract bool AcceptType(Type valueType);

        protected abstract Type GetConverterType(Type valueType);
    }
}