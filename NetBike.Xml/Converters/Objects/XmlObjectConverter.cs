namespace NetBike.Xml.Converters.Objects
{
    using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Contracts;
using Collections;

    public class XmlObjectConverter : IXmlConverter
    {
        public virtual bool CanRead(Type valueType)
        {
            return !valueType.IsBasicType() && valueType.IsActivable();
        }

        public virtual bool CanWrite(Type valueType)
        {
            return !valueType.IsBasicType();
        }

        public void WriteXml(XmlWriter writer, object value, XmlSerializationContext context)
        {
            if (value == null)
            {
                return;
            }

            if (context.Member.MappingType != XmlMappingType.Element)
            {
                throw new XmlSerializationException(string.Format("XML mapping of \"{0}\" must be Element.", context.ValueType));
            }

            var target = value;
            var contract = context.Contract.ToObjectContract();

            foreach (var property in contract.Properties)
            {
                if (CanWriteProperty(property))
                {
                    var propertyValue = GetValue(value, property);

                    if (!property.IsCollection)
                    {
                        context.Serialize(writer, propertyValue, property);
                    }
                    else
                    {
                        context.SerializeBody(writer, propertyValue, property);
                    }
                }
            }
        }

        public object ReadXml(XmlReader reader, XmlSerializationContext context)
        {
            if (context.Member.MappingType != XmlMappingType.Element)
            {
                throw new XmlSerializationException(string.Format("XML mapping of \"{0}\" must be Element.", context.ValueType));
            }

            var contract = context.Contract.ToObjectContract();
            var target = CreateTarget(contract);

            var propertyInfos = XmlPropertyInfo.GetInfo(contract, reader.NameTable, context);

            if (reader.MoveToFirstAttribute())
            {
                do
                {
                    ReadProperty(reader, target, propertyInfos, XmlMappingType.Attribute, context);
                }
                while (reader.MoveToNextAttribute());

                reader.MoveToElement();
            }

            if (!reader.IsEmptyElement)
            {
                if (contract.InnerTextProperty != null)
                {
                    var value = context.Deserialize(reader, contract.InnerTextProperty);
                    SetValue(target, contract.InnerTextProperty, value);
                }
                else
                {
                    reader.ReadStartElement();

                    while (reader.NodeType != XmlNodeType.EndElement)
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            ReadProperty(reader, target, propertyInfos, XmlMappingType.Element, context);
                        }
                        else
                        {
                            reader.Read();
                        }
                    }

                    reader.Read();
                }
            }
            else
            {
                reader.Read();
            }

            SetDefaultProperties(contract, target, propertyInfos);

            return GetResult(target);
        }

        protected virtual bool CanWriteProperty(XmlProperty property)
        {
            return property.HasGetterAndSetter;
        }

        protected virtual void OnUnknownProperty(XmlReader reader, object target, XmlSerializationContext context)
        {
            if (reader.NodeType == XmlNodeType.Element)
            {
                reader.Skip();
            }
        }

        protected virtual object CreateTarget(XmlContract contract)
        {
            return contract.CreateDefault();
        }

        protected virtual object GetValue(object target, XmlProperty property)
        {
            return property.GetValue(target);
        }

        protected virtual void SetValue(object target, XmlProperty property, object propertyValue)
        {
            property.SetValue(target, propertyValue);
        }

        protected virtual object GetResult(object target)
        {
            return target;
        }

        private void ReadProperty(XmlReader reader, object target, XmlPropertyInfo[] propertyInfos, XmlMappingType mappingType, XmlSerializationContext context)
        {
            for (var i = 0; i < propertyInfos.Length; i++)
            {
                var member = propertyInfos[i].Match(mappingType, reader);

                if (member != null)
                {
                    if (propertyInfos[i].CollectionProxy == null)
                    {
                        var value = context.Deserialize(reader, member);
                        SetValue(target, propertyInfos[i].Property, value);
                    }
                    else
                    {
                        var value = context.Deserialize(reader, propertyInfos[i].Property.Item);
                        propertyInfos[i].CollectionProxy.Add(value);
                    }

                    propertyInfos[i].Assigned = true;
                    return;
                }
            }

            OnUnknownProperty(reader, target, context);
        }

        private void SetDefaultProperties(XmlContract contract, object target, XmlPropertyInfo[] propertyInfos)
        {
            for (var i = 0; i < propertyInfos.Length; i++)
            {
                var propertyState = propertyInfos[i];

                if (!propertyState.Assigned)
                {
                    var property = propertyState.Property;

                    if (property.DefaultValue != null && property.PropertyInfo.CanWrite)
                    {
                        SetValue(target, property, property.DefaultValue);
                    }
                    else if (property.IsRequired)
                    {
                        throw new XmlSerializationException(string.Format("Property \"{0}\" of type \"{1}\" is required.", property.PropertyName, contract.ValueType));
                    }
                }
                else if (propertyState.CollectionProxy != null)
                {
                    var collection = propertyState.CollectionProxy.GetResult();
                    SetValue(target, propertyState.Property, collection);
                }
            }
        }
    }
}