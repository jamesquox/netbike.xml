﻿namespace NetBike.Xml.Converters.Basics
{
    using System;

    public sealed class XmlCustomDateTimeConverter : XmlBasicConverter<DateTime>
    {
        private readonly string format;

        public XmlCustomDateTimeConverter(string format = null)
        {
            this.format = format;
        }

        public string Format
        {
            get { return format; }
        }

        protected override DateTime Parse(string value, XmlSerializationContext context)
        {
            return DateTime.ParseExact(value, format, context.Settings.Culture);
        }

        protected override string ToString(DateTime value, XmlSerializationContext context)
        {
            return value.ToString(format, context.Settings.Culture);
        }
    }
}