using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting.DataSets;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.TestCommon;
using Xunit;
using Xunit.Extensions;
using Assert = Microsoft.TestCommon.AssertEx;
using System.Collections.Specialized;

namespace System.Net.Http.Formatting
{
    public class FormDataCollectionTests
    {
        [Fact]
        public void CreateFromUri()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&y=2"));

            Assert.Equal("1", form.Get("x"));
            Assert.Equal("2", form.Get("y"));
        }

        [Fact]
        public void CreateFromEmptyUri()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com"));

            Assert.Empty(form);            
        }

        [Fact]
        public void UriConstructorThrowsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new FormDataCollection((Uri)null));
        }


        [Fact]
        public void CreateFromEmptyString()
        {
            FormDataCollection form = new FormDataCollection("");

            Assert.Empty(form);       
        }

        [Fact]
        public void CreateFromNullString()
        {
            FormDataCollection form = new FormDataCollection((string) null);

            Assert.Empty(form);       
        }

        [Fact]
        public void PairConstructorThrowsNull()
        {
            var arg = (IEnumerable<KeyValuePair<string, string>>)null;
            Assert.Throws<ArgumentNullException>(() => new FormDataCollection(arg));
        }

        [Fact]
        public void CreateFromPairs()
        {
            Dictionary<string, string> pairs = new Dictionary<string,string> 
            { 
                { "x",  "1"}, 
                { "y" , "2"} 
            };

            var form = new FormDataCollection(pairs);

            Assert.Equal("1", form.Get("x"));
            Assert.Equal("2", form.Get("y"));
        }

        [Fact]
        public void Enumeration()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&y=2"));

            // Enumeration should be ordered
            String s = "";
            foreach (KeyValuePair<string, string> kv in form)
            {
                s += string.Format("{0}={1};", kv.Key, kv.Value);
            }

            Assert.Equal("x=1;y=2;", s);
        }

        [Fact]
        public void GetValues()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&x=2&x=3"));
                        
            Assert.Equal(new string [] { "1", "2", "3"}, form.GetValues("x"));
        }

        [Fact]
        public void CaseSensitive()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1&X=2"));

            NameValueCollection nvc = form.ReadAsNameValueCollection();

            Assert.Equal(2, nvc.Count);
            Assert.Equal("1", nvc.Get("x"));
            Assert.Equal("2", nvc.Get("X"));
        }

        [Fact]
        public void ToNameValueCollection()
        {
            FormDataCollection form = new FormDataCollection(new Uri("http://foo.com/?x=1a&y=2&x=1b&=ValueOnly&KeyOnly"));

            NameValueCollection nvc = form.ReadAsNameValueCollection();

            // y=2
            // x=1a;x=1b
            // =ValueOnly
            // KeyOnly
            Assert.Equal(4, nvc.Count);
            Assert.Equal(new string[] { "1a", "1b"}, nvc.GetValues("x"));
            Assert.Equal("1a,1b", nvc.Get("x"));
            Assert.Equal("2", nvc.Get("y"));
            Assert.Equal("", nvc.Get("KeyOnly"));            
            Assert.Equal("ValueOnly", nvc.Get(""));
        }
    }
}
