namespace NetBike.Xml.Contracts.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public sealed class XmlPropertyBuilderCollection : IEnumerable<XmlPropertyBuilder>
    {
        private readonly List<XmlPropertyBuilder> items;

        public XmlPropertyBuilderCollection()
        {
            items = new List<XmlPropertyBuilder>();
        }

        public XmlPropertyBuilderCollection(IEnumerable<XmlPropertyBuilder> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            this.items = new List<XmlPropertyBuilder>(items);
        }

        public int Count
        {
            get { return items.Count; }
        }

        public void Add(XmlPropertyBuilder item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var index = IndexOf(item.PropertyInfo);

            if (index != -1)
            {
                throw new ArgumentException(string.Format("Property \"{0}\" allready registered.", item.PropertyInfo), "item");
            }

            items.Add(item);
        }

        public void Set(XmlPropertyBuilder item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            var index = IndexOf(item.PropertyInfo);

            if (index == -1)
            {
                items.Add(item);
            }
            else
            {
                items[index] = item;
            }
        }

        public bool Contains(PropertyInfo propertyInfo)
        {
            return IndexOf(propertyInfo) != -1;
        }

        public bool Remove(PropertyInfo propertyInfo)
        {
            var index = IndexOf(propertyInfo);

            if (index != -1)
            {
                items.RemoveAt(index);
                return true;
            }

            return false;
        }

        public IEnumerable<XmlProperty> Build()
        {
            return items.Select(x => x.Build());
        }

        public IEnumerator<XmlPropertyBuilder> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return items.GetEnumerator();
        }

        internal static XmlPropertyBuilderCollection Create(IEnumerable<XmlProperty> properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }

            var items = properties.Select(x => XmlPropertyBuilder.Create(x));
            return new XmlPropertyBuilderCollection(items);
        }

        private int IndexOf(PropertyInfo propertyInfo)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (items[i].PropertyInfo == propertyInfo)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}