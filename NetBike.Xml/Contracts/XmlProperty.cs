namespace NetBike.Xml.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Reflection;
    using Builders;
    using Utilities;

    public class XmlProperty : XmlMember
    {
        private readonly bool isRequired;
        private readonly int order;
        private readonly PropertyInfo propertyInfo;
        private readonly bool hasGetterAndSetter;
        private readonly bool isCollection;
        private Action<object, object> setter;
        private Func<object, object> getter;

        public XmlProperty(
            PropertyInfo propertyInfo,
            XmlName name,
            XmlMappingType mappingType = XmlMappingType.Element,
            bool isRequired = false,
            XmlTypeHandling? typeHandling = null,
            XmlNullValueHandling? nullValueHandling = null,
            XmlDefaultValueHandling? defaultValueHandling = null,
            object defaultValue = null,
            XmlItem item = null,
            IEnumerable<XmlKnownType> knownTypes = null,
            bool isCollection = false,
            int order = -1)
            : base(propertyInfo.PropertyType, name, mappingType, typeHandling, nullValueHandling, defaultValueHandling, defaultValue, item, knownTypes)
        {
            if (isCollection)
            {
                if (!propertyInfo.PropertyType.IsEnumerable())
                {
                    throw new ArgumentException("Collection flag is available only for the IEnumerable type.");
                }

                this.isCollection = true;
            }

            this.propertyInfo = propertyInfo;
            this.isRequired = isRequired;
            this.order = order;
            hasGetterAndSetter = propertyInfo.CanRead && propertyInfo.CanWrite;
        }

        public PropertyInfo PropertyInfo
        {
            get { return propertyInfo; }
        }

        public string PropertyName
        {
            get { return propertyInfo.Name; }
        }

        public bool IsRequired
        {
            get { return isRequired; }
        }

        public bool IsCollection
        {
            get { return isCollection; }
        }

        public int Order
        {
            get { return order; }
        }

        internal bool HasGetterAndSetter
        {
            get { return hasGetterAndSetter; }
        }
        
        internal object GetValue(object target)
        {
            if (getter == null)
            {
                getter = DynamicWrapperFactory.CreateGetter(propertyInfo);
            }

            return getter(target);
        }

        internal void SetValue(object target, object value)
        {
            if (setter == null)
            {
                setter = DynamicWrapperFactory.CreateSetter(propertyInfo);
            }

            setter(target, value);
        }
    }
}