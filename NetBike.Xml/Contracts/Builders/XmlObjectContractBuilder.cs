namespace NetBike.Xml.Contracts.Builders
{
    using System;

    public class XmlObjectContractBuilder : XmlContractBuilder, IXmlCollectionBuilder
    {
        public XmlObjectContractBuilder(Type valueType)
            : base(valueType)
        {
        }

        public XmlTypeHandling? TypeHandling { get; set; }

        public XmlPropertyBuilderCollection Properties { get; set; }

        public XmlItemBuilder Item { get; set; }

        public static XmlObjectContractBuilder Create(XmlObjectContract contract)
        {
            if (contract == null)
            {
                throw new ArgumentNullException("contract");
            }

            return new XmlObjectContractBuilder(contract.ValueType)
            {
                Name = contract.Name,
                TypeHandling = contract.TypeHandling,
                Properties = XmlPropertyBuilderCollection.Create(contract.Properties),
                Item = contract.Item != null ? XmlItemBuilder.Create(contract.Item) : null
            };
        }

        public XmlObjectContract Build()
        {
            return new XmlObjectContract(
                ValueType,
                Name ?? ValueType.GetShortName(),
                Properties != null ? Properties.Build() : null,
                TypeHandling,
                Item != null ? Item.Build() : null);
        }

        public override XmlContract BuildContract()
        {
            return Build();
        }
    }
}