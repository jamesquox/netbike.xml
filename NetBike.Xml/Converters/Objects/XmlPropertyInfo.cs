﻿namespace NetBike.Xml.Converters.Objects
{
    using System.Xml;
    using Contracts;
    using Collections;

    internal struct XmlPropertyInfo
    {
        public XmlNameRef NameRef;
        public XmlProperty Property;
        public XmlNameRef[] KnownNameRefs;
        public ICollectionProxy CollectionProxy;
        public XmlMember Item;
        public bool Assigned;

        public static XmlPropertyInfo[] GetInfo(XmlObjectContract contract, XmlNameTable nameTable, XmlSerializationContext context)
        {
            var propertyInfos = new XmlPropertyInfo[contract.Properties.Count];

            for (var i = 0; i < propertyInfos.Length; i++)
            {
                var property = contract.Properties[i];
                propertyInfos[i].Property = property;

                if (!property.IsCollection)
                {
                    propertyInfos[i].NameRef.Reset(property.Name, nameTable);
                    propertyInfos[i].KnownNameRefs = XmlItemInfo.GetKnownNameRefs(property, nameTable);
                }
                else
                {
                    var typeContext = context.Settings.GetTypeContext(property.ValueType);
                    var collectionConverter = typeContext.ReadConverter as XmlCollectionConverter;

                    if (collectionConverter == null)
                    {
                        throw new XmlSerializationException(string.Format("Readable collection converter for the type \"{0}\" is not found.", property.ValueType));
                    }

                    var item = property.Item ?? typeContext.Contract.Root;

                    propertyInfos[i].CollectionProxy = collectionConverter.CreateProxy(property.ValueType);
                    propertyInfos[i].Item = item;
                    propertyInfos[i].NameRef.Reset(item.Name, nameTable);
                    propertyInfos[i].KnownNameRefs = XmlItemInfo.GetKnownNameRefs(item, nameTable);
                }
            }

            return propertyInfos;
        }

        public XmlMember Match(XmlMappingType mappingType, XmlReader reader)
        {
            if (mappingType == Property.MappingType)
            {
                if (NameRef.Match(reader))
                {
                    return Property;
                }

                if (KnownNameRefs != null)
                {
                    for (var i = 0; i < KnownNameRefs.Length; i++)
                    {
                        if (KnownNameRefs[i].Match(reader))
                        {
                            return (Item ?? Property).KnownTypes[i];
                        }
                    }
                }
            }

            return null;
        }
    }
}