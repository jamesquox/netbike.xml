using System.Runtime.Serialization;

namespace NetBike.Xml
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Contracts;
    using Converters;
    using Converters.Basics;
    using Converters.Collections;
    using Converters.Objects;
    using Converters.Specialized;
    using TypeResolvers;

    public sealed class XmlSerializerSettings
    {
        private static readonly XmlConverterCollection DefaultConverters;

        private readonly ConcurrentDictionary<Type, XmlTypeContext> typeContextCache;
        private readonly XmlConverterCollection converters;
        private readonly List<XmlNamespace> namespaces;
        private bool omitXmlDeclaration;
        private bool indent;
        private string indentChars;
        private Encoding encoding;
        private IXmlTypeResolver typeResolver;
        private IXmlContractResolver contractResolver;
        private CultureInfo cultureInfo;
        private XmlName typeAttributeName;
        private XmlName nullAttributeName;
        private XmlReaderSettings readerSettings;
        private XmlWriterSettings writerSettings;
        internal bool IsBuildingObjectGraph;
        static XmlSerializerSettings()
        {
            DefaultConverters = new XmlConverterCollection
            {
                new XmlStringConverter(),
                new XmlBooleanConverter(),
                new XmlCharConverter(),
                new XmlByteConverter(),
                new XmlSByteConverter(),
                new XmlInt16Converter(),
                new XmlUInt16Converter(),
                new XmlInt32Converter(),
                new XmlUInt32Converter(),
                new XmlInt64Converter(),
                new XmlUInt64Converter(),
                new XmlSingleConverter(),
                new XmlDoubleConverter(),
                new XmlDecimalConverter(),
                new XmlEnumConverter(),
                new XmlGuidConverter(),
                new XmlDateTimeConverter(),
                new XmlTimeSpanConverter(),
                new XmlDateTimeOffsetConverter(),
                new XmlArrayConverter(),
                new XmlListConverter(),
                new XmlDictionaryConverter(),
                new XmlKeyValuePairConverter(),
                new XmlOptionalConverter(),
                new XmlNullableConverter(),
                new XmlEnumerableConverter(),
                new XmlObjectConverter()
            };
        }

        public XmlSerializerSettings()
        {
            converters = new XmlConverterCollection();
            converters.CollectionChanged += (sender, ea) => typeContextCache.Clear();
            typeContextCache = new ConcurrentDictionary<Type, XmlTypeContext>();
            typeResolver = new XmlTypeResolver();
            contractResolver = new XmlContractResolver();
            cultureInfo = CultureInfo.InvariantCulture;
            typeAttributeName = new XmlName("type", XmlNamespace.Xsi);
            nullAttributeName = new XmlName("nil", XmlNamespace.Xsi);
            encoding = Encoding.UTF8;
            TypeHandling = XmlTypeHandling.Auto;
            NullValueHandling = XmlNullValueHandling.Ignore;
            NoneValueHandling = XmlNoneValueHandling.Ignore;
            DefaultValueHandling = XmlDefaultValueHandling.Include;
            ReferenceHandling = XmlReferenceHandling.Throw;
            ReferenceHandlingIdName = "id";
            ReferenceHandlingReferenceName = "ref";
            ReferenceHandlingGenerator = new ObjectIDGenerator();
            EmptyCollectionHandling = XmlEmptyCollectionHandling.Include;
            omitXmlDeclaration = false;
            indentChars = "  ";
            indent = false;
            namespaces = new List<XmlNamespace>
            {
                new XmlNamespace("xsi", XmlNamespace.Xsi)
            };
        }


        public XmlTypeHandling TypeHandling { get; set; }

        public XmlNullValueHandling NullValueHandling { get; set; }

        /// <summary>
        /// How to handle <see cref="OptionalSharp.Optional{T}.HasValue"/> values.
        /// </summary>
        public XmlNoneValueHandling NoneValueHandling { get; set; }

        public XmlDefaultValueHandling DefaultValueHandling { get; set; }

        public XmlReferenceHandling ReferenceHandling { get; set; }

        public XmlReferenceExpansion ReferenceExpansion { get; set; }

        public string ReferenceHandlingIdName { get; set; }

        public string ReferenceHandlingReferenceName { get; set; }

        public ObjectIDGenerator ReferenceHandlingGenerator { get; set; }

        public XmlEmptyCollectionHandling EmptyCollectionHandling { get; set; }

        public bool OmitXmlDeclaration
        {
            get
            {
                return omitXmlDeclaration;
            }

            set
            {
                omitXmlDeclaration = value;
                readerSettings = null;
            }
        }

        public bool Indent
        {
            get
            {
                return indent;
            }

            set
            {
                indent = value;
                readerSettings = null;
            }
        }

        public string IndentChars
        {
            get
            {
                return indentChars;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                indentChars = value;
                readerSettings = null;
            }
        }

        public XmlName TypeAttributeName
        {
            get
            {
                return typeAttributeName;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                typeAttributeName = value;
            }
        }

        public XmlName NullAttributeName
        {
            get
            {
                return nullAttributeName;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                nullAttributeName = value;
            }
        }

        public CultureInfo Culture
        {
            get
            {
                return cultureInfo;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                cultureInfo = value;
            }
        }

        public IXmlTypeResolver TypeResolver
        {
            get
            {
                return typeResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                typeResolver = value;
                typeContextCache.Clear();
            }
        }

        public IXmlContractResolver ContractResolver
        {
            get
            {
                return contractResolver;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                contractResolver = value;
                typeContextCache.Clear();
            }
        }

        public Encoding Encoding
        {
            get
            {
                return encoding;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                encoding = value;
                readerSettings = null;
            }
        }

        public ICollection<XmlNamespace> Namespaces
        {
            get { return namespaces; }
        }

        public ICollection<IXmlConverter> Converters
        {
            get { return converters; }
        }

        internal XmlWriterSettings GetWriterSettings()
        {
            var settings = writerSettings;

            if (settings == null)
            {
                settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = OmitXmlDeclaration,
                    Indent = Indent,
                    Encoding = Encoding,
                    IndentChars = IndentChars,
                    CloseOutput = false
                };

                writerSettings = settings;
            }

            return settings;
        }

        internal XmlReaderSettings GetReaderSettings()
        {
            var settings = readerSettings;

            if (settings == null)
            {
                settings = new XmlReaderSettings
                {
                    IgnoreComments = true,
                    IgnoreProcessingInstructions = true,
                    IgnoreWhitespace = true,
                    CloseInput = false
                };

                readerSettings = settings;
            }

            return settings;
        }

        internal XmlTypeContext GetTypeContext(Type valueType)
        {
            XmlTypeContext context;

            if (!typeContextCache.TryGetValue(valueType, out context))
            {
                context = CreateTypeContext(valueType, context);
            }

            return context;
        }

        private static IXmlConverter GetConverter(XmlContract contract, IXmlConverter converter)
        {
            if (converter == null)
            {
                return null;
            }

            var factory = converter as IXmlConverterFactory;

            if (factory != null)
            {
                converter = factory.CreateConverter(contract);
            }

            return converter;
        }

        private XmlTypeContext CreateTypeContext(Type valueType, XmlTypeContext context)
        {
            IXmlConverter readConverter = null;
            IXmlConverter writeConverter = null;

            foreach (var converter in converters.Concat(DefaultConverters))
            {
                if (readConverter == null && converter.CanRead(valueType))
                {
                    readConverter = converter;

                    if (writeConverter != null)
                    {
                        break;
                    }
                }

                if (writeConverter == null && converter.CanWrite(valueType))
                {
                    writeConverter = converter;

                    if (readConverter != null)
                    {
                        break;
                    }
                }
            }

            var contract = contractResolver.ResolveContract(valueType);

            readConverter = GetConverter(contract, readConverter);
            writeConverter = GetConverter(contract, writeConverter);

            context = new XmlTypeContext(contract, readConverter, writeConverter);
            typeContextCache.TryAdd(valueType, context);
            return context;
        }
    }
}