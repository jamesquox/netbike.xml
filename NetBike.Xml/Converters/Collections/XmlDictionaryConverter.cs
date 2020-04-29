﻿namespace NetBike.Xml.Converters.Collections
{
    using System;
    using System.Collections.Generic;
    using Utilities;

    public sealed class XmlDictionaryConverter : XmlConverterFactory
    {
        protected override bool AcceptType(Type valueType)
        {
            return valueType.IsGenericTypeOf(typeof(Dictionary<,>), typeof(IDictionary<,>));
        }

        protected override Type GetConverterType(Type valueType)
        {
            return typeof(XmlTypedDictionaryConverter<,>).MakeGenericType(valueType.GetGenericArguments());
        }

        private sealed class XmlTypedDictionaryConverter<TKey, TValue> : XmlCollectionConverter
        {
            public override bool CanRead(Type valueType)
            {
                return valueType == typeof(Dictionary<TKey, TValue>) || valueType == typeof(IDictionary<TKey, TValue>);
            }

            public override ICollectionProxy CreateProxy(Type valueType)
            {
                return new DictionaryProxy();
            }

            private sealed class DictionaryProxy : ICollectionProxy
            {
                private Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

                public void Add(object value)
                {
                    var keyValuePair = (KeyValuePair<TKey, TValue>)value;
                    dictionary.Add(keyValuePair.Key, keyValuePair.Value);
                }

                public object GetResult()
                {
                    return dictionary;
                }
            }
        }
    }
}