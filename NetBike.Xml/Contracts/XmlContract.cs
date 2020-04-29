namespace NetBike.Xml.Contracts
{
    using System;
    using Utilities;

    public class XmlContract
    {
        private readonly Type valueType;
        private readonly XmlName name;
        private XmlMember root;
        private Func<object> creator;

        public XmlContract(Type valueType, XmlName name)
        {
            if (valueType == null)
            {
                throw new ArgumentNullException("valueType");
            }

            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            this.valueType = valueType;
            this.name = name;
        }

        public Type ValueType
        {
            get { return valueType; }
        }

        public XmlName Name
        {
            get { return name; }
        }

        internal XmlMember Root
        {
            get
            {
                if (root == null)
                {
                    root = GetDefaultMember();
                }

                return root;
            }
        }

        internal object CreateDefault()
        {
            if (creator == null)
            {
                creator = DynamicWrapperFactory.CreateConstructor(valueType);
            }

            return creator();
        }

        protected virtual XmlMember GetDefaultMember()
        {
            return new XmlMember(
                valueType,
                name,
                XmlMappingType.Element,
                XmlTypeHandling.None,
                XmlNullValueHandling.Include,
                XmlDefaultValueHandling.Include);
        }
    }
}