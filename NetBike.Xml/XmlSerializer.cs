namespace NetBike.Xml
{
    using System;
    using System.IO;
    using System.Xml;

    public sealed class XmlSerializer
    {
        private readonly XmlSerializerSettings settings;

        public XmlSerializer()
            : this(new XmlSerializerSettings())
        {
        }

        public XmlSerializer(XmlSerializerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException("settings");
            }

            this.settings = settings;
        }

        public XmlSerializerSettings Settings
        {
            get { return settings; }
        }

        public bool CanSerialize(Type valueType)
        {
            return settings.GetTypeContext(valueType).WriteConverter != null;
        }

        public bool CanDeserialize(Type valueType)
        {
            return settings.GetTypeContext(valueType).ReadConverter != null;
        }

        public void Serialize<T>(Stream stream, T value)
        {
            Serialize(stream, typeof(T), value);
        }

        public void Serialize<T>(TextWriter output, T value)
        {
            Serialize(output, typeof(T), value);
        }

        public void Serialize<T>(XmlWriter writer, T value)
        {
            Serialize(writer, typeof(T), value);
        }

        public void Serialize(Stream stream, Type valueType, object value)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var writer = XmlWriter.Create(stream, settings.GetWriterSettings());
            Serialize(writer, valueType, value);
        }

        public void Serialize(TextWriter output, Type valueType, object value)
        {
            if (output == null)
            {
                throw new ArgumentNullException("output");
            }

            var writer = XmlWriter.Create(output, settings.GetWriterSettings());
            Serialize(writer, valueType, value);
        }

        public void Serialize(XmlWriter writer, Type valueType, object value)
        {
            var context = new XmlSerializationContext(settings);
            context.Serialize(writer, value, valueType);
            writer.Flush();
        }

        public T Deserialize<T>(Stream stream)
        {
            return (T)Deserialize(stream, typeof(T));
        }

        public T Deserialize<T>(TextReader input)
        {
            return (T)Deserialize(input, typeof(T));
        }

        public T Deserialize<T>(XmlReader reader)
        {
            return (T)Deserialize(reader, typeof(T));
        }

        public object Deserialize(Stream stream, Type valueType)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var reader = XmlReader.Create(stream, settings.GetReaderSettings());
            return Deserialize(reader, valueType);
        }

        public object Deserialize(TextReader input, Type valueType)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var reader = XmlReader.Create(input, settings.GetReaderSettings());
            return Deserialize(reader, valueType);
        }

        public object Deserialize(XmlReader reader, Type valueType)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }

            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            var context = new XmlSerializationContext(settings);
            return context.Deserialize(reader, valueType);
        }
    }
}