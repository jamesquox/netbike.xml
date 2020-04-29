namespace NetBike.Xml
{
    using System.Xml;
    using Contracts;

    internal struct XmlNameRef
    {
        public object LocalName;
        public object NamespaceUri;

        public XmlNameRef(XmlName name, XmlNameTable nameTable)
        {
            LocalName = nameTable.Add(name.LocalName);
            NamespaceUri = name.NamespaceUri != null ? nameTable.Add(name.NamespaceUri) : null;
        }

        public void Reset(XmlName name, XmlNameTable nameTable)
        {
            LocalName = nameTable.Add(name.LocalName);
            NamespaceUri = name.NamespaceUri != null ? nameTable.Add(name.NamespaceUri) : null;
        }

        public bool Match(XmlReader reader)
        {
            if (ReferenceEquals(LocalName, reader.LocalName))
            {
                return NamespaceUri == null || ReferenceEquals(NamespaceUri, reader.NamespaceURI);
            }

            return false;
        }
    }
}