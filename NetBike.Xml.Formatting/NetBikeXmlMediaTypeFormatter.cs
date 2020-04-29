using System.Xml.Serialization;

namespace NetBike.Xml.Formatting
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
    using System.Text;
    using Xml;

    public sealed class NetBikeXmlMediaTypeFormatter : BufferedMediaTypeFormatter
    {
        private readonly XmlSerializer serializer;

        public NetBikeXmlMediaTypeFormatter()
            : this(CreateDefaultSettings())
        {
        }

        public NetBikeXmlMediaTypeFormatter(XmlSerializerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            serializer = new XmlSerializer(settings);

            SupportedEncodings.Add(new UTF8Encoding(false, true));
            SupportedEncodings.Add(new UnicodeEncoding(false, true, true));

            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/xml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xml"));
        }

        public XmlSerializerSettings Settings
        {
            get
            {
                return serializer.Settings;
            }
        }

        public override bool CanReadType(Type type)
        {
            return serializer.CanDeserialize(type);
        }

        public override bool CanWriteType(Type type)
        {
            return serializer.CanSerialize(type);
        }

        public override object ReadFromStream(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            return serializer.Deserialize(readStream, type);
        }

        public override void WriteToStream(Type type, object value, Stream writeStream, HttpContent content)
        {
            serializer.Serialize(writeStream, type, value);
        }

        private static XmlSerializerSettings CreateDefaultSettings()
        {
            return new XmlSerializerSettings();
        }
    }
}