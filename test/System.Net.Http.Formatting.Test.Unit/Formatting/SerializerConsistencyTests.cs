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

        [Fact]
        public void ClassWithIenumerable()
        {
            var widget = new ClassWithIenumerable { Property = "something" };
            SerializerConsistencyHepers.Test(widget); // XML fails to serialize
        }

        [Fact]
        public void ClassWithIenumerableAndDataContract()
        {
            var widget = new ClassWithIenumerable2 { Property = "something" };
            SerializerConsistencyHepers.Test(widget); // XML fails to serialize
        }

        [Fact]
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

        [Fact]
        public void DerivedProperties()
        {
            BaseClass source = new DerivedClass{ Property = "base", DerivedProperty = "derived" };
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
    }

    class SerializerConsistencyHepers
    {
        public static void Work()
        {   
        }

        // Exercise the various serialization paths to verify that the default serializers behave consistently.
        public static void Test(object source)
        {
            Type tSource = source.GetType();
            Test(source, tSource);
        }

        public static void Test(object source, Type tSource)
        {
            MediaTypeFormatter mXml = new MediaTypeFormatterCollection().XmlFormatter;
            MediaTypeFormatter mJson = new MediaTypeFormatterCollection().JsonFormatter;

            MemoryStream blobXml = null;
            MemoryStream blobJson = null;
                                    
            blobJson = Write(source, tSource, mJson); // C# --> JSON
            blobXml = Write(source, tSource, mXml); // C# --> XML
            
            object obj1;
            object obj2;
            obj1 = Read(blobXml, tSource, mXml); // C# --> XML --> C#            
            obj2 = Read(blobJson, tSource, mJson); // C# --> JSON --> C#
            
            // We were able to round trip the source object through both formatters.
            // Now see if the resulting object is the same.

            // Check C# --> XML --> C#

            var blobXml2 = Write(obj1, tSource, mXml);  // C# --> XML
            var blobJson2 = Write(obj1, tSource, mJson); // C# --> JSON

            Compare(blobXml, blobXml2);
            Compare(blobJson, blobJson2);

            // Check C# --> JSON --> C#

            var blobXml3 = Write(obj2, tSource, mXml);  // C# --> XML
            var blobJson3 = Write(obj2, tSource, mJson); // C# --> JSON

            Compare(blobXml, blobXml3);
            Compare(blobJson, blobJson3);
        }

        static void Compare(MemoryStream ms1, MemoryStream ms2)
        {
            string s1 = ToString(ms1);
            string s2 = ToString(ms2);
            
            Assert.Equal(s1, s2);            
        }

        static string ToString(MemoryStream ms)
        {
            byte[] b = ms.GetBuffer();
            return System.Text.Encoding.UTF8.GetString(b, 0, (int)ms.Length);
        }

        static object Read(MemoryStream ms, Type tSource, MediaTypeFormatter formatter)
        {
            bool f = formatter.CanReadType(tSource);
            Assert.True(f);

            HttpContentHeaders hd = null;
            IFormatterLogger logger = null;
            object o = formatter.ReadFromStreamAsync(tSource, ms, hd, logger).Result;
            Assert.True(tSource.IsAssignableFrom(o.GetType()));

            return o;
        }

        static MemoryStream Write(object obj, Type tSource, MediaTypeFormatter formatter)
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
