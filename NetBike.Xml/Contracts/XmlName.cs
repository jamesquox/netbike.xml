﻿namespace NetBike.Xml.Contracts
{
    using System;
    using System.Xml;

    public sealed class XmlName : IEquatable<XmlName>
    {
        private readonly string localName;
        private readonly string namespaceUri;

        public XmlName(string localName)
            : this(localName, null)
        {
        }

        public XmlName(string localName, string namespaceUri)
        {
            if (localName == null)
            {
                throw new ArgumentNullException("localName");
            }

            XmlConvert.VerifyNCName(localName);

            if (namespaceUri != null)
            {
                XmlNamespace.VerifyNamespaceUri(namespaceUri);
            }

            this.localName = localName;
            this.namespaceUri = namespaceUri;
        }

        public string LocalName
        {
            get { return localName; }
        }

        public string NamespaceUri
        {
            get { return namespaceUri; }
        }

        public static implicit operator XmlName(string name)
        {
            return new XmlName(name);
        }

        public override string ToString()
        {
            if (namespaceUri != null)
            {
                return "{" + namespaceUri + "}" + localName;
            }

            return localName;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as XmlName);
        }

        public override int GetHashCode()
        {
            var hashCode = 17;

            unchecked
            {
                hashCode = 31 * hashCode + localName.GetHashCode();

                if (namespaceUri != null)
                {
                    hashCode = 31 * hashCode + namespaceUri.GetHashCode();
                }
            }

            return hashCode;
        }

        public bool Equals(XmlName other)
        {
            if (other == null)
            {
                return false;
            }

            return localName == other.localName && namespaceUri == other.namespaceUri;
        }

        internal XmlName Create(string localName, string namespaceUri)
        {
            if (string.IsNullOrEmpty(localName))
            {
                localName = this.localName;
            }

            if (string.IsNullOrEmpty(namespaceUri))
            {
                namespaceUri = this.namespaceUri;
            }

            return new XmlName(localName, namespaceUri);
        }
    }
}