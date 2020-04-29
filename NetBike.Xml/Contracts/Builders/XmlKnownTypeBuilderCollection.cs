namespace NetBike.Xml.Contracts.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class XmlKnownTypeBuilderCollection : IEnumerable<XmlKnownTypeBuilder>
    {
        private readonly List<XmlKnownTypeBuilder> items;

        public XmlKnownTypeBuilderCollection()
        {
            items = new List<XmlKnownTypeBuilder>();
        }

        public XmlKnownTypeBuilderCollection(IEnumerable<XmlKnownTypeBuilder> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.items = new List<XmlKnownTypeBuilder>(items);
        }

        public int Count
        {
            get { return items.Count; }
        }

        public void Add(Type valueType, XmlName name)
        {
            var builder = new XmlKnownTypeBuilder(valueType).SetName(name);
            Add(builder);
        }

        public void Add(XmlKnownTypeBuilder item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var index = IndexOf(item.ValueType);

            if (index == -1)
            {
                throw new ArgumentException(string.Format("Known type \"{0}\" allready registered.", item.ValueType));
            }

            Add(item);
        }

        public void Set(XmlKnownTypeBuilder item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var index = IndexOf(item.ValueType);

            if (index == -1)
            {
                items.Add(item);
            }
            else
            {
                items[index] = item;
            }
        }

        public bool Contains(Type valueType)
        {
            return IndexOf(valueType) != -1;
        }

        public bool Remove(Type valueType)
        {
            var index = IndexOf(valueType);

            if (index != -1)
            {
                items.RemoveAt(index);
                return true;
            }

            return false;
        }

        public IEnumerable<XmlKnownType> Build()
        {
            return items.Select(x => x.Build());
        }

        public IEnumerator<XmlKnownTypeBuilder> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        internal static XmlKnownTypeBuilderCollection Create(IEnumerable<XmlKnownType> knownTypes)
        {
            if (knownTypes == null)
            {
                throw new ArgumentNullException("knownTypes");
            }

            var items = knownTypes.Select(x => XmlKnownTypeBuilder.Create(x));
            return new XmlKnownTypeBuilderCollection(items);
        }

        private int IndexOf(Type valueType)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].ValueType == valueType)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}