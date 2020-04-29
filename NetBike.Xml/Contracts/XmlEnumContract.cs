namespace NetBike.Xml.Contracts
{
    using System;
    using System.Collections.Generic;
    using Builders;

    public sealed class XmlEnumContract : XmlContract
    {
        private readonly Type underlyingType;
        private readonly bool isFlag;
        private readonly List<XmlEnumItem> items;

        public XmlEnumContract(Type valueType, XmlName name, IEnumerable<XmlEnumItem> items)
            : base(valueType, name)
        {
            if (items == null)
            {
                throw new ArgumentNullException("items");
            }

            if (!valueType.IsEnum)
            {
                throw new ArgumentException("Expected enum type.", "valueType");
            }

            this.items = new List<XmlEnumItem>(items);
            isFlag = valueType.IsDefined(Types.FlagsAttribute, false);
            underlyingType = Enum.GetUnderlyingType(valueType);
        }

        public Type UnderlyingType
        {
            get { return underlyingType; }
        }

        public IEnumerable<XmlEnumItem> Items
        {
            get { return items; }
        }

        public bool IsFlag
        {
            get { return isFlag; }
        }
    }
}