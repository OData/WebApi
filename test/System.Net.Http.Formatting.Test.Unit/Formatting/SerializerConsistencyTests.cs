using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http.Formatting;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using Xunit;

namespace System.Net.Formatting.Tests
{
    // Tests for ensuring the serializers behave consistently in various cases. 
    // This is important for conneg. 
    public class SerializerConsistencyTests
    {
        [Fact]
        public void PartialContract()
        {
            var c = new PartialDataContract { PropertyWithAttribute = "one", PropertyWithoutAttribute = "false" };
            SerializerConsistencyHepers.Test(c);
        }

        [Fact]
        public void ClassWithFields()
        {
            var c1 = new ClassWithFields { Property = "prop" };
            c1.SetField("field");
            SerializerConsistencyHepers.Test(c1);
        }

        [Fact(Skip = "failing")]
        public void ClassWithIenumerable()
        {
            var widget = new ClassWithIenumerable { Property = "something" };
            SerializerConsistencyHepers.Test(widget); // XML fails to serialize
        }

        [Fact(Skip = "failing")]
        public void ClassWithIenumerableAndDataContract()
        {
            var widget = new ClassWithIenumerable2 { Property = "something" };
            SerializerConsistencyHepers.Test(widget); // XML fails to serialize
        }

        [Fact(Skip = "failing")]
        public void TestAnonymousType()
        {
            var anonymous = new { X = 10, Y = 15 };
            SerializerConsistencyHepers.Test(anonymous); // XML fails to write anonymous types
        }

        [Fact]
        public void PrivateProperty()
        {
            var source2 = new PrivateProperty { FirstName = "John", LastName = "Smith" };
            source2.SetItem("shoes");
            SerializerConsistencyHepers.Test(source2);
        }

        [Fact]
        public void NormalClass()
        {
            var source = new NormalClass { FirstName = "John", LastName = "Smith", Item = "Socks" };
            SerializerConsistencyHepers.Test(source);
        }

        [Fact(Skip = "failing")]
        public void DerivedProperties()
        {
            BaseClass source = new DerivedClass { Property = "base", DerivedProperty = "derived" };
            SerializerConsistencyHepers.Test(source, typeof(BaseClass));
        }

        [Fact]
        public void Dictionary()
        {
            var dict = new Dictionary<string, int>();
            dict["one"] = 1;
            dict["two"] = 2;

            SerializerConsistencyHepers.Test(dict);
        }

        [Fact]
        public void Array()
        {
            string[] array = new string[] { "First", "Second", "Last" };

            SerializerConsistencyHepers.Test(array);
        }

        [Fact]
        public void ArrayInterfaces()
        {
            string[] array = new string[] { "First", "Second", "Last" };

            SerializerConsistencyHepers.Test(array, typeof(IList<string>));
            SerializerConsistencyHepers.Test(array, typeof(ICollection<string>));
            SerializerConsistencyHepers.Test(array, typeof(IEnumerable<string>));
        }

        [Fact(Skip = "failing")]
        public void LinqDirect()
        {
            var l = from i in Enumerable.Range(1, 10) where i > 5 select i * i;

            // Write as the derived runtime type, but then read back as just an IEnumerable.
            SerializerConsistencyHepers.Test(l, tSourceWrite: l.GetType(), tSourceRead: typeof(IEnumerable<int>));
        }

        [Fact]
        public void Linq()
        {
            var l = from i in Enumerable.Range(1, 10) where i > 5 select i * i;

            // Runtime type of a linq expression is some derived Linq type which we can't deserialize to. 
            // So explicitly call out IEnumerable<T>
            SerializerConsistencyHepers.Test(l, typeof(IEnumerable<int>));
        }
    }

    // public class, public fields
    public class NormalClass
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Item { get; set; }
    }

    [DataContract]
    public class PartialDataContract
    {
        [DataMember]
        public string PropertyWithAttribute { get; set; }

        // no attribute here
        public string PropertyWithoutAttribute { get; set; }        
    }

    public class PrivateProperty // with private field
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        private string Item { get; set; }

        public void SetItem(string item)
        {
            this.Item = item;
        }
    }

    public class ClassWithFields
    {
        public string Property { get; set; }
        private string Field;

        public void SetField(string field)
        {
            this.Field = field;
        }
    }

    public class BaseClass
    {
        public string Property { get; set; }
    }

    public class DerivedClass : BaseClass
    {
        public string DerivedProperty { get; set; }
    }

    // Does a serializer see this implements IEnumerable? And does it treat it specially?
    public class ClassWithIenumerable2 : IEnumerable<string>
    {
        public string Property { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return GetEnumeratorWorker();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumeratorWorker();
        }

        private IEnumerator<string> GetEnumeratorWorker()
        {
            string[] vals = new string[] { "First", "Second", "Third" };
            IEnumerable<string> e = vals;
            return e.GetEnumerator();
        }
    }

    // Enumerable, decorated with [DataContract] attributes.
    [DataContract]
    public class ClassWithIenumerable : IEnumerable<string>
    {
        [DataMember]
        public string Property { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return GetEnumeratorWorker();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumeratorWorker();
        }

        private IEnumerator<string> GetEnumeratorWorker()
        {
            string[] vals = new string[] { "First", "Second", "Third" };
            IEnumerable<string> e = vals;
            return e.GetEnumerator();
        }
    }

    // Helpers for performing consistency checks with the serializers.
    class SerializerConsistencyHepers
    {
        // Exercise the various serialization paths to verify that the default serializers behave consistently.
        public static void Test(object source)
        {
            Type tSource = source.GetType();
            Test(source, tSource);
        }

        // Allow explicitly passing in the type that gets passed to the serializer. 
        // The expectation is that the type can be read and written with both serializers. 
        public static void Test(object source, Type tSource)
        {
            Test(source, tSource, tSource);
        }

        // tSourceWrite - the type we use for the initial write.  This can be specific, and a 1-way serializable type (eg, a linq expression). 
        // tSourceRead - the type that we read back as. This should be more general because we need to instantiate it.
        public static void Test(object source, Type tSourceWrite, Type tSourceRead)
        {
            MediaTypeFormatter mXml = new MediaTypeFormatterCollection().XmlFormatter;
            MediaTypeFormatter mJson = new MediaTypeFormatterCollection().JsonFormatter;

            MemoryStream blobXml = null;
            MemoryStream blobJson = null;

            blobJson = Write(source, tSourceWrite, mJson); // C# --> JSON
            blobXml = Write(source, tSourceWrite, mXml); // C# --> XML
            
            object obj1;
            object obj2;
            obj2 = Read(blobJson, tSourceRead, mJson); // C# --> JSON --> C#
            obj1 = Read(blobXml, tSourceRead, mXml); // C# --> XML --> C#            
            
            // We were able to round trip the source object through both formatters.
            // Now see if the resulting object is the same.

            // Check C# --> XML --> C#

            var blobXml2 = Write(obj1, tSourceRead, mXml);  // C# --> XML
            var blobJson2 = Write(obj1, tSourceRead, mJson); // C# --> JSON

            Compare(blobXml, blobXml2);
            Compare(blobJson, blobJson2);

            // Check C# --> JSON --> C#

            var blobXml3 = Write(obj2, tSourceRead, mXml);  // C# --> XML
            var blobJson3 = Write(obj2, tSourceRead, mJson); // C# --> JSON

            Compare(blobXml, blobXml3);
            Compare(blobJson, blobJson3);
        }

        // Compare if 2 streams have the same contents. 
        private static void Compare(MemoryStream ms1, MemoryStream ms2)
        {
            string s1 = ToString(ms1);
            string s2 = ToString(ms2);
            
            Assert.Equal(s1, s2);            
        }

        // Given a memory stream (which is representing a textual serialization format), get the string.
        private static string ToString(MemoryStream ms)
        {
            byte[] b = ms.GetBuffer();
            return System.Text.Encoding.UTF8.GetString(b, 0, (int)ms.Length);
        }

        private static object Read(MemoryStream ms, Type tSource, MediaTypeFormatter formatter)
        {
            bool f = formatter.CanReadType(tSource);
            Assert.True(f);

            HttpContentHeaders hd = null;
            IFormatterLogger logger = null;
            object o = formatter.ReadFromStreamAsync(tSource, ms, hd, logger).Result;
            Assert.True(tSource.IsAssignableFrom(o.GetType()));

            return o;
        }

        private static MemoryStream Write(object obj, Type tSource, MediaTypeFormatter formatter)
        {
            bool f = formatter.CanWriteType(tSource);
            Assert.True(f);

            MemoryStream ms = new MemoryStream();

            HttpContentHeaders hd = null;
            formatter.WriteToStreamAsync(tSource, obj, ms, hd, null).Wait();

            ms.Position = 0;
            return ms;
        }
    }
}
