namespace NetBike.Xml.Converters.Collections
{
    using System.Xml;
    using Contracts;

    internal struct XmlItemInfo
    {
        public XmlNameRef NameRef;
        public XmlItem Item;
        public XmlNameRef[] KnownNameRefs;

        public XmlItemInfo(XmlItem item, XmlNameTable nameTable)
        {
            Item = item;
            NameRef = new XmlNameRef(item.Name, nameTable);
            KnownNameRefs = GetKnownNameRefs(item, nameTable);
        }

        public XmlMember Match(XmlReader reader)
        {
            if (NameRef.Match(reader))
            {
                return Item;
            }

            if (KnownNameRefs != null)
            {
                for (int i = 0; i < KnownNameRefs.Length; i++)
                {
                    if (KnownNameRefs[i].Match(reader))
                    {
                        return Item.KnownTypes[i];
                    }
                }
            }

            return null;
        }

        internal static XmlNameRef[] GetKnownNameRefs(XmlMember item, XmlNameTable nameTable)
        {
            if (item.KnownTypes.Count == 0)
            {
                return null;
            }

            var nameRefs = new XmlNameRef[item.KnownTypes.Count];

            for (var i = 0; i < nameRefs.Length; i++)
            {
                nameRefs[i].Reset(item.KnownTypes[i].Name, nameTable);
            }

            return nameRefs;
        }
    }
}