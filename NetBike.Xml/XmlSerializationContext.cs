using System.Linq;

namespace NetBike.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Contracts;
    using OptionalSharp;

    public sealed class XmlSerializationContext
    {
        private readonly XmlSerializerSettings settings;
        private XmlContract currentContract;
        private XmlMember currentMember;
        private int currentLevel;
        private bool initialState;
        private Dictionary<string, object> properties;
        private XmlNameRef typeNameRef;
        private XmlNameRef nullNameRef;
        private XmlReader lastUsedReader;
        private Dictionary<long, int> _xmlReferenceLevelLookup;

        public XmlSerializationContext(XmlSerializerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.settings = settings;
            initialState = true;
        }

        internal XmlSerializationContext(XmlSerializerSettings settings, XmlMember member, XmlContract contract)
            : this(settings)
        {
            if (contract == null)
            {
                throw new ArgumentNullException("contract");
            }

            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            currentContract = contract;
            currentMember = member;
            initialState = false;
        }

        public Type ValueType => currentContract.ValueType;

        public XmlContract Contract => currentContract;

        public XmlMember Member => currentMember;

        public IDictionary<string, object> Properties
        {
            get
            {
                if (properties == null)
                {
                    properties = new Dictionary<string, object>();
                }

                return properties;
            }
        }

        public XmlSerializerSettings Settings => settings;

        public XmlContract GetTypeContract(Type valueType)
        {
            return settings.GetTypeContext(valueType).Contract;
        }

        public void Serialize(XmlWriter writer, object value, Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (settings.ReferenceExpansion == XmlReferenceExpansion.HighestLevel)
            {
                // build object graph for later consumption
                _xmlReferenceLevelLookup = new Dictionary<long, int>();
                Settings.IsBuildingObjectGraph = true;
                Serialize(writer, value, valueType, null);
                Settings.IsBuildingObjectGraph = false;
            }

            Serialize(writer, value, valueType, null);
        }

        public void Serialize(XmlWriter writer, object value, XmlMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("memberInfo");
            }

            Serialize(writer, value, member.ValueType, member);
        }

        public object Deserialize(XmlReader reader, Type valueType)
        {
            return Deserialize(reader, valueType, null);
        }

        public object Deserialize(XmlReader reader, XmlMember member)
        {
            return Deserialize(reader, member.ValueType, member);
        }

        public void SerializeBody(XmlWriter writer, object value, Type valueType)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            SerializeBody(writer, value, valueType, null);
        }

        public void SerializeBody(XmlWriter writer, object value, XmlMember member)
        {
            if (member == null)
            {
                throw new ArgumentNullException("member");
            }

            SerializeBody(writer, value, member.ValueType, member);
        }

        internal void WriteTypeName(XmlWriter writer, Type valueType)
        {
            // don't write to writer if building object graph
            if (Settings.IsBuildingObjectGraph)
            {
                return;
            }

            string typeName = settings.TypeResolver.GetTypeName(valueType);
            writer.WriteAttributeString(settings.TypeAttributeName, typeName);
        }

        internal void WriteNull(XmlWriter writer, Type valueType, XmlMember member)
        {
            // don't write to writer if building object graph
            if (Settings.IsBuildingObjectGraph)
            {
                return;
            }

            XmlNullValueHandling nullValueHandling = settings.NullValueHandling;

            if (member != null)
            {
                if (member.MappingType == XmlMappingType.Attribute)
                {
                    return;
                }

                nullValueHandling = member.NullValueHandling ?? nullValueHandling;
            }

            if (nullValueHandling != XmlNullValueHandling.Ignore)
            {
                if (member == null)
                {
                    member = settings.GetTypeContext(valueType).Contract.Root;
                }

                writer.WriteStartElement(member.Name);

                if (initialState)
                {
                    initialState = false;
                    WriteNamespaces(writer);
                }

                writer.WriteAttributeString(settings.NullAttributeName, "true");
                writer.WriteEndElement();
            }
        }

        internal bool ReadValueType(XmlReader reader, ref Type valueType)
        {
            if (reader.AttributeCount > 0)
            {
                if (!ReferenceEquals(lastUsedReader, reader))
                {
                    typeNameRef.Reset(settings.TypeAttributeName, reader.NameTable);
                    nullNameRef.Reset(settings.NullAttributeName, reader.NameTable);
                    lastUsedReader = reader;
                }

                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        if (nullNameRef.Match(reader))
                        {
                            return false;
                        }
                        else if (typeNameRef.Match(reader))
                        {
                            valueType = settings.TypeResolver.ResolveTypeName(valueType, reader.Value);
                        }
                    }
                    while (reader.MoveToNextAttribute());

                    reader.MoveToElement();
                }
            }

            return true;
        }

        internal bool TryResolveValueType(object value, ref XmlMember member, out Type valueType)
        {
            if (member.IsOpenType)
            {
                XmlTypeHandling typeHandling = member.TypeHandling ?? settings.TypeHandling;

                if (typeHandling != XmlTypeHandling.None)
                {
                    valueType = value.GetType();
                    member = member.ResolveMember(valueType);
                    return typeHandling == XmlTypeHandling.Always || valueType != member.ValueType;
                }
            }

            valueType = member.ValueType;

            return false;
        }

        internal void WriteXml(XmlWriter writer, object value, XmlMember member, XmlTypeContext typeContext)
        {
            XmlMember lastMember = currentMember;
            XmlContract lastContract = currentContract;

            currentMember = member;
            currentContract = typeContext.Contract;

            typeContext.WriteXml(writer, value, this);

            currentMember = lastMember;
            currentContract = lastContract;
        }

        internal object ReadXml(XmlReader reader, XmlMember member, XmlTypeContext typeContext)
        {
            XmlMember lastMember = currentMember;
            XmlContract lastContract = currentContract;

            currentMember = member;
            currentContract = typeContext.Contract;

            object value = typeContext.ReadXml(reader, this);

            currentMember = lastMember;
            currentContract = lastContract;

            return value;
        }

        private void SerializeBody(XmlWriter writer, object value, Type memberType, XmlMember member)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            if (value == null)
            {
                WriteNull(writer, memberType, member);
            }
            else
            {
                XmlTypeContext typeContext = settings.GetTypeContext(memberType);
                WriteXml(writer, value, member ?? typeContext.Contract.Root, typeContext);
            }
        }

        private void Serialize(XmlWriter writer, object value, Type memberType, XmlMember member)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            // ignore optional values with none value
            if (value is IAnyOptional optional)
            {
                if (!optional.HasValue)
                {
                    if (Settings.NoneValueHandling == XmlNoneValueHandling.Ignore)
                    {
                        return;
                    }
                    // Output default values for inner types
                    var innerType = optional.GetInnerType();
                    var nullableInnerType = innerType.GetUnderlyingNullableType();
                    if (nullableInnerType != null)
                    {
                        WriteNull(writer, memberType, member);
                        return;
                    }
                    else if (innerType.IsClass)
                    {
                        WriteNull(writer, memberType, member);
                        return;
                    }
                }
                else if (optional.Value == null)
                {
                    WriteNull(writer, memberType, member);
                    return;
                }
            }

            if (value == null)
            {
                WriteNull(writer, memberType, member);
                return;
            }

            if (settings.EmptyCollectionHandling == XmlEmptyCollectionHandling.Ignore && value.IsEmpty() == true)
            {
                return;
            }


            XmlTypeContext context = null;

            if (member == null)
            {
                context = settings.GetTypeContext(memberType);
                member = context.Contract.Root;
            }

            bool shouldWriteTypeName = TryResolveValueType(value, ref member, out Type valueType);

            if (member.DefaultValue != null)
            {
                XmlDefaultValueHandling defaultValueHandling = member.DefaultValueHandling ?? settings.DefaultValueHandling;

                if (defaultValueHandling == XmlDefaultValueHandling.Ignore && value.Equals(member.DefaultValue))
                {
                    return;
                }
            }

            if (context == null || context.Contract.ValueType != member.ValueType)
            {
                context = settings.GetTypeContext(valueType);
            }

            switch (member.MappingType)
            {
                case XmlMappingType.Element:
                    WriteElement(writer, value, member, context, valueType, shouldWriteTypeName);
                    break;

                case XmlMappingType.Attribute:
                    if (!Settings.IsBuildingObjectGraph)
                    {
                        writer.WriteStartAttribute(member.Name);
                    }

                    WriteXml(writer, value, member, context);

                    if (!Settings.IsBuildingObjectGraph)
                    {
                        writer.WriteEndAttribute();
                    }
                    break;

                case XmlMappingType.InnerText:
                    if (!Settings.IsBuildingObjectGraph)
                    {
                        WriteXml(writer, value, member, context);
                    }
                    break;
            }
        }

        internal void WriteElement(XmlWriter writer, object value, XmlMember member, XmlTypeContext context, Type valueType, bool shouldWriteTypeName)
        {
            if (!Settings.IsBuildingObjectGraph)
            {
                writer.WriteStartElement(member.Name);

                if (initialState)
                {
                    initialState = false;
                    WriteNamespaces(writer);
                }

                if (shouldWriteTypeName)
                {
                    WriteTypeName(writer, valueType);
                }
            }

            var isCollection = !context.Contract.ValueType.Equals(typeof(string)) &&
                context.Contract.ValueType.IsEnumerable();

            if (context.Contract is XmlObjectContract &&
                context.Contract.ValueType.IsClass &&
                !context.Contract.ValueType.IsOptional() &&
                !isCollection)
            {
                long id = settings.ReferenceHandlingGenerator.GetId(value, out bool firstTime);

                if (Settings.IsBuildingObjectGraph)
                {
                    if (firstTime)
                    {
                        _xmlReferenceLevelLookup.Add(id, currentLevel);
                        currentLevel++;
                        WriteXml(writer, value, member, context);
                        currentLevel--;
                    }
                    else
                    {
                        if (Settings.ReferenceHandling == XmlReferenceHandling.Throw)
                        {
                            throw new Exception("Found reference loop. Please set ReferenceHandling setting to XmlReferenceHandling.Handle");
                        }
                        if (currentLevel < _xmlReferenceLevelLookup[id])
                        {
                            _xmlReferenceLevelLookup[id] = currentLevel;
                        }
                    }
                    // stop processing
                    return;
                }

                bool isExpansionElement = Settings.ReferenceExpansion == XmlReferenceExpansion.FirstAccessed
                    ? firstTime
                    : currentLevel == _xmlReferenceLevelLookup[id];
                if (isExpansionElement)
                {
                    if (Settings.ReferenceExpansion == XmlReferenceExpansion.HighestLevel)
                    {
                        // element should only be expanded once - prevents expansion of elements on the same level
                        _xmlReferenceLevelLookup[id] = -1;
                    }
                    if (Settings.ReferenceHandling == XmlReferenceHandling.Handle)
                    {
                        writer.WriteAttributeString(Settings.ReferenceHandlingIdName, id.ToString());
                    }
                    currentLevel++;
                    WriteXml(writer, value, member, context);
                    currentLevel--;
                }
                else
                {
                    if (Settings.ReferenceHandling == XmlReferenceHandling.Throw)
                    {
                        throw new Exception("Found reference loop. Please set ReferenceHandling setting to XmlReferenceHandling.Handle");
                    }
                    writer.WriteAttributeString(Settings.ReferenceHandlingReferenceName, id.ToString());
                }
            }
            else
            {
                if (!Settings.IsBuildingObjectGraph || isCollection)
                {
                    WriteXml(writer, value, member, context);
                }
            }

            if (!Settings.IsBuildingObjectGraph)
            {
                writer.WriteEndElement();
            }
        }

        private object Deserialize(XmlReader reader, Type valueType, XmlMember member)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (initialState && reader.NodeType == XmlNodeType.None)
            {
                initialState = false;

                while (reader.NodeType != XmlNodeType.Element)
                {
                    if (!reader.Read())
                    {
                        return null;
                    }
                }
            }

            if (reader.NodeType == XmlNodeType.Element)
            {
                if (!ReadValueType(reader, ref valueType))
                {
                    reader.Skip();
                    return null;
                }
            }

            XmlTypeContext typeInfo = settings.GetTypeContext(valueType);

            if (member == null)
            {
                member = typeInfo.Contract.Root;
            }

            return ReadXml(reader, member, typeInfo);
        }

        private void WriteNamespaces(XmlWriter writer)
        {
            // don't write to writer if building object graph
            if (Settings.IsBuildingObjectGraph)
            {
                return;
            }

            foreach (XmlNamespace item in settings.Namespaces)
            {
                writer.WriteNamespace(item);
            }
        }
    }
}