using System.Collections;
using System.Linq;

namespace NetBike.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Xml;
    using Contracts;
    using Converters;

    public sealed class XmlSerializationContext
    {
        private readonly XmlSerializerSettings settings;
        private XmlContract currentContract;
        private XmlMember currentMember;
        private bool initialState;
        private Dictionary<string, object> properties;
        private XmlNameRef typeNameRef;
        private XmlNameRef nullNameRef;
        private XmlReader lastUsedReader;

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

        public Type ValueType
        {
            get { return currentContract.ValueType; }
        }

        public XmlContract Contract
        {
            get { return currentContract; }
        }

        public XmlMember Member
        {
            get { return currentMember; }
        }

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

        public XmlSerializerSettings Settings
        {
            get { return settings; }
        }

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
            var typeName = settings.TypeResolver.GetTypeName(valueType);
            writer.WriteAttributeString(settings.TypeAttributeName, typeName);
        }

        internal void WriteNull(XmlWriter writer, Type valueType, XmlMember member)
        {
            var nullValueHandling = settings.NullValueHandling;

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
                var typeHandling = member.TypeHandling ?? settings.TypeHandling;

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
            var lastMember = currentMember;
            var lastContract = currentContract;

            currentMember = member;
            currentContract = typeContext.Contract;

            typeContext.WriteXml(writer, value, this);

            currentMember = lastMember;
            currentContract = lastContract;
        }

        internal object ReadXml(XmlReader reader, XmlMember member, XmlTypeContext typeContext)
        {
            var lastMember = currentMember;
            var lastContract = currentContract;

            currentMember = member;
            currentContract = typeContext.Contract;

            var value = typeContext.ReadXml(reader, this);

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
                var typeContext = settings.GetTypeContext(memberType);
                WriteXml(writer, value, member ?? typeContext.Contract.Root, typeContext);
            }
        }

        private void Serialize(XmlWriter writer, object value, Type memberType, XmlMember member)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
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

            Type valueType;
            XmlTypeContext context = null;

            if (member == null)
            {
                context = settings.GetTypeContext(memberType);
                member = context.Contract.Root;
            }

            var shouldWriteTypeName = TryResolveValueType(value, ref member, out valueType);

            if (member.DefaultValue != null)
            {
                var defaultValueHandling = member.DefaultValueHandling ?? settings.DefaultValueHandling;

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

                    if (context.Contract is XmlObjectContract && !_objectIgnoreTypes.Contains(context.Contract.Name.LocalName))
                    {
                        var id = settings.ReferenceHandlingGenerator.GetId(value, out bool firstTime);
                        if (firstTime)
                        {
                            writer.WriteAttributeString(Settings.ReferenceHandlingIdName, id.ToString());
                            WriteXml(writer, value, member, context);
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
                        WriteXml(writer, value, member, context);
                    }

                    writer.WriteEndElement();
                    break;

                case XmlMappingType.Attribute:
                    writer.WriteStartAttribute(member.Name);
                    WriteXml(writer, value, member, context);
                    writer.WriteEndAttribute();
                    break;

                case XmlMappingType.InnerText:
                    WriteXml(writer, value, member, context);
                    break;
            }
        }

        private static string[] _objectIgnoreTypes = {"List", "Nullable", "ArrayOfByte" };

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

            var typeInfo = settings.GetTypeContext(valueType);

            if (member == null)
            {
                member = typeInfo.Contract.Root;
            }

            return ReadXml(reader, member, typeInfo);
        }

        private void WriteNamespaces(XmlWriter writer)
        {
            foreach (var item in settings.Namespaces)
            {
                writer.WriteNamespace(item);
            }
        }
    }
}