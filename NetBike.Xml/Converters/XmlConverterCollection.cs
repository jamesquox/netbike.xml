namespace NetBike.Xml.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class XmlConverterCollection : Collection<IXmlConverter>
    {
        public XmlConverterCollection()
        {
        }

        public XmlConverterCollection(IEnumerable<IXmlConverter> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            foreach (var item in items)
            {
                Add(item);
            }
        }

        public event EventHandler CollectionChanged;

        protected override void InsertItem(int index, IXmlConverter item)
        {
            base.InsertItem(index, item);
            OnCollectionChanged();
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            OnCollectionChanged();
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            OnCollectionChanged();
        }

        private void OnCollectionChanged()
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new EventArgs());
            }
        }
    }
}