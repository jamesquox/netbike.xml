﻿namespace NetBike.Xml.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class XmlMember
    {
        private static readonly List<XmlKnownType> EmptyKnownTypes = new List<XmlKnownType>();

        private readonly XmlName name;
        private readonly Type valueType;
        private readonly XmlItem item;
        private readonly XmlMappingType mappingType;
        private readonly XmlTypeHandling? typeHandling;
        private readonly XmlDefaultValueHandling? defaultValueHandling;
        private readonly XmlNullValueHandling? nullValueHandling;
        private readonly object defaultValue;
        private readonly bool isOpenType;
        private readonly List<XmlKnownType> knownTypes;

        internal XmlMember(
            Type valueType,
            XmlName name,
            XmlMappingType mappingType = XmlMappingType.Element,
            XmlTypeHandling? typeHandling = null,
            XmlNullValueHandling? nullValueHandling = null,
            XmlDefaultValueHandling? defaultValueHandling = null,
            object defaultValue = null,
            XmlItem item = null,
            IEnumerable<XmlKnownType> knownTypes = null)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            var isFinalType = valueType.IsFinalType();

            this.valueType = valueType;
            this.name = name;
            this.typeHandling = typeHandling;
            this.defaultValueHandling = defaultValueHandling;
            this.defaultValue = defaultValue;
            isOpenType = !isFinalType && valueType.IsVisible;
            this.item = item;
            this.nullValueHandling = nullValueHandling;
            this.mappingType = mappingType;

            if (knownTypes != null)
            {
                var count = knownTypes.Count();

                if (count > 0)
                {
                    if (mappingType != XmlMappingType.Element)
                    {
                        throw new ArgumentException("Known types may be set only for XML Element.", "knownTypes");
                    }

                    if (isFinalType)
                    {
                        throw new ArgumentException("Known types cannot be set for final value type.", "knownTypes");
                    }

                    this.knownTypes = new List<XmlKnownType>(count);

                    foreach (var knownType in knownTypes)
                    {
                        if (valueType == knownType.valueType)
                        {
                            throw new ArgumentException(string.Format("Known type \"{0}\" cannot be equal to the value type.", valueType), "knownTypes");
                        }

                        if (!valueType.IsAssignableFrom(knownType.ValueType))
                        {
                            throw new ArgumentException(string.Format("Known type \"{0}\" must be inherits from \"{1}\".", knownType.ValueType, valueType), "knownTypes");
                        }

                        this.knownTypes.Add(knownType);
                    }
                }
            }
        }

        public Type ValueType
        {
            get { return valueType; }
        }

        public XmlName Name
        {
            get { return name; }
        }

        public XmlMappingType MappingType
        {
            get { return mappingType; }
        }

        public XmlTypeHandling? TypeHandling
        {
            get { return typeHandling; }
        }

        public XmlNullValueHandling? NullValueHandling
        {
            get { return nullValueHandling; }
        }

        public XmlDefaultValueHandling? DefaultValueHandling
        {
            get { return defaultValueHandling; }
        }

        public object DefaultValue
        {
            get { return defaultValue; }
        }

        public XmlItem Item
        {
            get { return item; }
        }

        public IReadOnlyList<XmlKnownType> KnownTypes
        {
            get { return knownTypes ?? EmptyKnownTypes; }
        }

        internal bool IsOpenType
        {
            get { return isOpenType; }
        }

        internal XmlMember ResolveMember(Type valueType)
        {
            if (knownTypes != null)
            {
                foreach (var item in knownTypes)
                {
                    if (item.valueType == valueType)
                    {
                        return item;
                    }
                }
            }

            return this;
        }
    }
}