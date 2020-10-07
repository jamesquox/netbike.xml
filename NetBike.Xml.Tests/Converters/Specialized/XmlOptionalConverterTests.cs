namespace NetBike.Xml.Tests.Converters.Specialized
{
    using System;
    using System.Xml.Serialization;
    using Contracts;
    using NetBike.Xml.Converters.Specialized;
    using XmlUnit.NUnitAdapter;
    using NUnit.Framework;
    using XmlSerializer = XmlSerializer;
    using OptionalSharp;
    using System.Collections.Generic;
    using System.Text;

    [TestFixture]
    public class XmlOptionalConverterTests
    {
        [Test]
        public void CanReadTest()
        {
            var converter = new XmlOptionalConverter();
            Assert.IsTrue(converter.CanRead(typeof(Optional<int>)));
            Assert.IsTrue(converter.CanRead(typeof(Optional<DateTime>)));
            Assert.IsFalse(converter.CanRead(typeof(DateTime)));
            Assert.IsFalse(converter.CanRead(typeof(Optional<>)));
        }

        [Test]
        public void CanWriteTest()
        {
            var converter = new XmlOptionalConverter();
            Assert.IsTrue(converter.CanWrite(typeof(Optional<int>)));
            Assert.IsTrue(converter.CanWrite(typeof(Optional<DateTime>)));
            Assert.IsFalse(converter.CanWrite(typeof(DateTime)));
            Assert.IsFalse(converter.CanWrite(typeof(Optional<>)));
        }

        [Test]
        public void WriteNullableTest()
        {
            var converter = new XmlOptionalConverter();
            var actual = converter.ToXml<Optional<int?>>(1);
            var expected = "<xml>1</xml>";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteNullableAttributeTest()
        {
            var converter = new XmlOptionalConverter();
            var actual = converter.ToXml<Optional<int?>>(1, member: GetAttributeMember<int?>());
            var expected = "<xml value=\"1\" />";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteNullTest()
        {
            var converter = new XmlOptionalConverter();
            var actual = converter.ToXml<Optional<int?>>(null);
            var expected = "<xml />";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteNoneTest()
        {
            var converter = new XmlOptionalConverter();
            var actual = converter.ToXml<Optional<int?>>(default);
            var expected = "<xml />";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteNullAttributeTest()
        {
            var serializer = new XmlSerializer();
            var actual = serializer.ToXml(new PatchRoot());
            var expected = @"<PatchRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><AutoId>db93b213-d703-4da4-8011-6f82a9495406</AutoId></PatchRoot>";
            Assert.That(actual, IsXml.Equals(expected).WithIgnoreDeclaration());
        }

        [Test]
        public void WriteNullWithNullIncludeHandlingTest()
        {
            var serializer = new XmlSerializer();
            serializer.Settings.OmitXmlDeclaration = true;
            serializer.Settings.NullValueHandling = XmlNullValueHandling.Include;
            var actual = serializer.ToXml(new PatchRoot());
            var expected = @"<PatchRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Ref3 xsi:nil=""true"" /><Name xsi:nil=""true"" /><AutoId>db93b213-d703-4da4-8011-6f82a9495406</AutoId><Children xsi:nil=""true"" /></PatchRoot>";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteNullWithNoneIncludeHandlingTest()
        {
            var serializer = new XmlSerializer();
            serializer.Settings.OmitXmlDeclaration = true;
            serializer.Settings.NoneValueHandling = XmlNoneValueHandling.Include;
            var actual = serializer.ToXml(new PatchRoot());
            var expected = @"<PatchRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Id>0</Id><AutoId2>00000000-0000-0000-0000-000000000000</AutoId2><AutoId>db93b213-d703-4da4-8011-6f82a9495406</AutoId></PatchRoot>";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteNullWithNullIncludeAndNoneIncludeHandlingTest()
        {
            var serializer = new XmlSerializer();
            serializer.Settings.OmitXmlDeclaration = true;
            serializer.Settings.NullValueHandling = XmlNullValueHandling.Include;
            serializer.Settings.NoneValueHandling = XmlNoneValueHandling.Include;
            var actual = serializer.ToXml(new PatchRoot());
            var expected = @"<PatchRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><Id>0</Id><Ref xsi:nil=""true"" /><Ref3 xsi:nil=""true"" /><Name xsi:nil=""true"" /><AutoId2>00000000-0000-0000-0000-000000000000</AutoId2><AutoId>db93b213-d703-4da4-8011-6f82a9495406</AutoId><Children xsi:nil=""true"" /><OptionalChildren xsi:nil=""true"" /></PatchRoot>";
            Assert.That(actual, IsXml.Equals(expected));
        }

        [Test]
        public void WriteComplex()
        {
            var serializer = new XmlSerializer();
            serializer.Settings.TypeHandling = XmlTypeHandling.None;
            serializer.Settings.DefaultValueHandling = XmlDefaultValueHandling.Include;
            serializer.Settings.NullValueHandling = XmlNullValueHandling.Include;
            serializer.Settings.NoneValueHandling = XmlNoneValueHandling.Ignore;
            serializer.Settings.ReferenceHandling = XmlReferenceHandling.Handle;
            serializer.Settings.ReferenceExpansion = XmlReferenceExpansion.HighestLevel;
            serializer.Settings.EmptyCollectionHandling = XmlEmptyCollectionHandling.Ignore;
            serializer.Settings.Encoding = Encoding.Unicode;
            serializer.Settings.Converters.Add(new XmlByteArrayConverter());

            var source = new PatchRoot
            {
                Id = 56,
                Name = "Root",
                Ref = 887,
                Children = new List<PatchChild>
                {
                    null,
                    new PatchChild
                    {
                        Id = 1,
                        Ref = 1,
                        Name = "Child 1"
                    },
                    new PatchChild(),
                    new PatchChild
                    {
                        AutoId = Guid.Parse("d602385f-913b-432f-bbb9-ca339be629a6")
                    }
                },
                OptionalChildren = new List<Optional<PatchChild>>
                {
                    null,
                    Optional.NoneOf<PatchChild>()
                }
            };

            source.Children.ForEach(q => source.OptionalChildren.Do(w => w.Add(q)));

            var actual = serializer.ToXml(source);
            var expected = @"<?xml version=""1.0"" encoding=""utf-16""?><PatchRoot xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" id=""1""><Id>56</Id><Ref>887</Ref><Ref3 xsi:nil=""true"" /><Name>Root</Name><AutoId>db93b213-d703-4da4-8011-6f82a9495406</AutoId><Children><PatchChild xsi:nil=""true"" /><PatchChild id=""2""><Id>1</Id><Ref>1</Ref><Name>Child 1</Name><AutoId>00000000-0000-0000-0000-000000000000</AutoId></PatchChild><PatchChild id=""3""><AutoId>00000000-0000-0000-0000-000000000000</AutoId></PatchChild><PatchChild id=""4""><AutoId>d602385f-913b-432f-bbb9-ca339be629a6</AutoId></PatchChild></Children><OptionalChildren><PatchChild xsi:nil=""true"" /><PatchChild xsi:nil=""true"" /><PatchChild ref=""2"" /><PatchChild ref=""3"" /><PatchChild ref=""4"" /></OptionalChildren></PatchRoot>";
            Assert.That(actual, IsXml.Equals(expected));
        }

        //[Test]
        //public void ReadNullableTest()
        //{
        //    var serializer = new XmlSerializer();
        //    var xml = "<TestClass><Value>1</Value></TestClass>";
        //    var actual = serializer.ParseXml<PatchRoot>(xml);
        //    Optional<int?> expected = 1;
        //    Assert.AreEqual(expected, actual);
        //}

        //[Test]
        //public void ReadNullTest()
        //{
        //    var serializer = new XmlSerializer();
        //    var xml = "<testClass></testClass>";
        //    var actual = serializer.ParseXml<PatchRoot>(xml);
        //    Optional<int?> expected = null;
        //    Assert.AreEqual(expected, actual);
        //}

        //[Test]
        //public void ReadNullWithNullIncludeHandlingTest()
        //{
        //    var serializer = new XmlSerializer();
        //    var xml = @"<testClass><value xsi:nil=""true"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" /></testClass>";
        //    var actual = serializer.ParseXml<PatchRoot>(xml);
        //    Optional<int?> expected = null;
        //    Assert.AreEqual(expected, actual);
        //}

        private static XmlMember GetAttributeMember<T>()
        {
            return new XmlMember(typeof(T), "value", XmlMappingType.Attribute);
        }

        public class PatchRoot
        {
            public Optional<int> Id { get; set; }
            public Optional<int?> Ref { get; set; }
            public int? Ref3 { get; set; }
            public string Name { get; set; }
            public Optional<Guid> AutoId2 { get; set; }
            public Optional<Guid> AutoId { get; set; } = Guid.Parse("db93b213-d703-4da4-8011-6f82a9495406");
            public List<PatchChild> Children { get; set; }
            public Optional<List<Optional<PatchChild>>> OptionalChildren { get; set; }
        }
        public class PatchChild
        {
            public Optional<int> Id { get; set; }
            public Optional<int?> Ref { get; set; }
            public Optional<string> Name { get; set; }
            public Guid AutoId { get; set; }
        }
    }
}