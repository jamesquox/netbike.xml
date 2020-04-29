﻿namespace NetBike.Xml.Contracts.Builders
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    public sealed class XmlPropertyBuilder : XmlItemBuilder, IXmlObjectBuilder, IXmlCollectionBuilder
    {
        public XmlPropertyBuilder(PropertyInfo propertyInfo)
            : base(propertyInfo.PropertyType)
        {
            PropertyInfo = propertyInfo;
            MappingType = XmlMappingType.Element;
            Order = -1;
        }

        public PropertyInfo PropertyInfo { get; private set; }

        public XmlMappingType MappingType { get; set; }

        public XmlNullValueHandling? NullValueHandling { get; set; }

        public XmlDefaultValueHandling? DefaultValueHandling { get; set; }

        public object DefaultValue { get; set; }

        public int Order { get; set; }

        public XmlItemBuilder Item { get; set; }

        public bool IsCollection { get; set; }

        public bool IsRequired { get; set; }

        public static XmlPropertyBuilder Create(Type ownerType, string propertyName)
        {
            var propertyInfo = GetPropertyInfo(ownerType, propertyName);
            return new XmlPropertyBuilder(propertyInfo);
        }

        public static XmlPropertyBuilder Create<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> expression)
        {
            var propertyInfo = GetPropertyInfo(expression);
            return new XmlPropertyBuilder(propertyInfo);
        }

        public static XmlPropertyBuilder Create(XmlProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            return new XmlPropertyBuilder(property.PropertyInfo)
            {
                Name = property.Name,
                MappingType = property.MappingType,
                NullValueHandling = property.NullValueHandling,
                TypeHandling = property.TypeHandling,
                DefaultValueHandling = property.DefaultValueHandling,
                DefaultValue = property.DefaultValue,
                IsRequired = property.IsRequired,
                IsCollection = property.IsCollection,
                Order = property.Order,
                Item = property.Item != null ? XmlItemBuilder.Create(property.Item) : null,
                KnownTypes = property.KnownTypes != null ? XmlKnownTypeBuilderCollection.Create(property.KnownTypes) : null
            };
        }

        public new XmlProperty Build()
        {
            return new XmlProperty(
                PropertyInfo,
                Name ?? PropertyInfo.Name,
                MappingType,
                IsRequired,
                TypeHandling,
                NullValueHandling,
                DefaultValueHandling,
                DefaultValue,
                Item != null ? Item.Build() : null,
                KnownTypes != null ? KnownTypes.Build() : null,
                IsCollection,
                Order);
        }

        internal static PropertyInfo GetPropertyInfo(Type ownerType, string propertyName)
        {
            if (ownerType == null)
            {
                throw new ArgumentNullException("ownerType");
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }

            var propertyInfo = ownerType.GetProperty(propertyName);

            if (propertyInfo == null)
            {
                throw new ArgumentException(string.Format("Property \"{0}\" is not declared in the type \"{1}\".", propertyName, ownerType), "propertyName");
            }

            return propertyInfo;
        }

        internal static PropertyInfo GetPropertyInfo<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var memberExpression = expression.Body as MemberExpression;

            if (memberExpression == null)
            {
                throw new ArgumentException("Expected property expression.");
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo == null)
            {
                throw new ArgumentException("Expected property expression.");
            }

            var ownerType = typeof(TOwner);

            if (propertyInfo.DeclaringType == ownerType)
            {
                return propertyInfo;
            }

            return ownerType.GetProperty(propertyInfo.Name);
        }
    }
}