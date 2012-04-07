// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.DataSets.Types
{
    public static class HttpTestData
    {
        public static readonly TestData<HttpMethod> AllHttpMethods = new RefTypeTestData<HttpMethod>(() =>
            StandardHttpMethods.Concat(CustomHttpMethods).ToList());

        public static readonly TestData<HttpMethod> StandardHttpMethods = new RefTypeTestData<HttpMethod>(() => new List<HttpMethod>() 
        { 
            HttpMethod.Head,
            HttpMethod.Get,
            HttpMethod.Post,
            HttpMethod.Put,
            HttpMethod.Delete,
            HttpMethod.Options,
            HttpMethod.Trace,
        });

        public static readonly TestData<HttpMethod> CustomHttpMethods = new RefTypeTestData<HttpMethod>(() => new List<HttpMethod>() 
        { 
            new HttpMethod("Custom")
        });

        public static readonly TestData<HttpStatusCode> AllHttpStatusCodes = new ValueTypeTestData<HttpStatusCode>(new HttpStatusCode[]
        {
            HttpStatusCode.Accepted,
            HttpStatusCode.Ambiguous,
            HttpStatusCode.BadGateway,
            HttpStatusCode.BadRequest,
            HttpStatusCode.Conflict,
            HttpStatusCode.Continue,
            HttpStatusCode.Created,
            HttpStatusCode.ExpectationFailed,
            HttpStatusCode.Forbidden,
            HttpStatusCode.Found,
            HttpStatusCode.GatewayTimeout,
            HttpStatusCode.Gone,
            HttpStatusCode.HttpVersionNotSupported,
            HttpStatusCode.InternalServerError,
            HttpStatusCode.LengthRequired,
            HttpStatusCode.MethodNotAllowed,
            HttpStatusCode.Moved,
            HttpStatusCode.MovedPermanently,
            HttpStatusCode.MultipleChoices,
            HttpStatusCode.NoContent,
            HttpStatusCode.NonAuthoritativeInformation,
            HttpStatusCode.NotAcceptable,
            HttpStatusCode.NotFound,
            HttpStatusCode.NotImplemented,
            HttpStatusCode.NotModified,
            HttpStatusCode.OK,
            HttpStatusCode.PartialContent,
            HttpStatusCode.PaymentRequired,
            HttpStatusCode.PreconditionFailed,
            HttpStatusCode.ProxyAuthenticationRequired,
            HttpStatusCode.Redirect,
            HttpStatusCode.RedirectKeepVerb,
            HttpStatusCode.RedirectMethod,
            HttpStatusCode.RequestedRangeNotSatisfiable,
            HttpStatusCode.RequestEntityTooLarge,
            HttpStatusCode.RequestTimeout,
            HttpStatusCode.RequestUriTooLong,
            HttpStatusCode.ResetContent,
            HttpStatusCode.SeeOther,
            HttpStatusCode.ServiceUnavailable,
            HttpStatusCode.SwitchingProtocols,
            HttpStatusCode.TemporaryRedirect,
            HttpStatusCode.Unauthorized,
            HttpStatusCode.UnsupportedMediaType,
            HttpStatusCode.Unused,
            HttpStatusCode.UseProxy
        });

        public static readonly TestData<HttpStatusCode> CustomHttpStatusCodes = new ValueTypeTestData<HttpStatusCode>(new HttpStatusCode[]
        {
            (HttpStatusCode)199,
            (HttpStatusCode)299,
            (HttpStatusCode)399,
            (HttpStatusCode)499,
            (HttpStatusCode)599,
            (HttpStatusCode)699,
            (HttpStatusCode)799,
            (HttpStatusCode)899,
            (HttpStatusCode)999,
        });

        public static readonly ReadOnlyCollection<TestData> ConvertablePrimitiveValueTypes = new ReadOnlyCollection<TestData>(new TestData[] {
            TestData.CharTestData, 
            TestData.IntTestData, 
            TestData.UintTestData, 
            TestData.ShortTestData, 
            TestData.UshortTestData, 
            TestData.LongTestData, 
            TestData.UlongTestData, 
            TestData.ByteTestData, 
            TestData.SByteTestData, 
            TestData.BoolTestData,
            TestData.DoubleTestData, 
            TestData.FloatTestData, 
            TestData.DecimalTestData, 
            TestData.TimeSpanTestData, 
            TestData.GuidTestData, 
            TestData.DateTimeTestData,
            TestData.DateTimeOffsetTestData});

        public static readonly ReadOnlyCollection<TestData> ConvertableEnumTypes = new ReadOnlyCollection<TestData>(new TestData[] {
            TestData.SimpleEnumTestData, 
            TestData.LongEnumTestData,
            TestData.FlagsEnumTestData, 
            DataContractEnumTestData});

        public static readonly ReadOnlyCollection<TestData> ConvertableValueTypes = new ReadOnlyCollection<TestData>(
            ConvertablePrimitiveValueTypes.Concat(ConvertableEnumTypes).ToList());

        public static readonly TestData<MediaTypeHeaderValue> StandardJsonMediaTypes = new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>() 
        { 
            new MediaTypeHeaderValue("application/json"),
            new MediaTypeHeaderValue("text/json")
        });

        public static readonly TestData<MediaTypeHeaderValue> StandardXmlMediaTypes = new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>() 
        { 
            new MediaTypeHeaderValue("application/xml"),
            new MediaTypeHeaderValue("text/xml")
        });

        public static readonly TestData<MediaTypeHeaderValue> StandardODataMediaTypes = new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>() 
        { 
            new MediaTypeHeaderValue("application/atom+xml"),
            new MediaTypeHeaderValue("application/json"),
        });

        public static readonly TestData<MediaTypeHeaderValue> StandardFormUrlEncodedMediaTypes = new RefTypeTestData<MediaTypeHeaderValue>(() => new List<MediaTypeHeaderValue>() 
        { 
            new MediaTypeHeaderValue("application/x-www-form-urlencoded")
        });

        public static readonly TestData<string> StandardJsonMediaTypeStrings = new RefTypeTestData<string>(() => new List<string>() 
        { 
            "application/json",
            "text/json"
        });

        public static readonly TestData<string> StandardXmlMediaTypeStrings = new RefTypeTestData<string>(() => new List<string>() 
        { 
            "application/xml",
            "text/xml"
        });

        public static readonly TestData<string> LegalMediaTypeStrings = new RefTypeTestData<string>(() =>
            StandardXmlMediaTypeStrings.Concat(StandardJsonMediaTypeStrings).ToList());


        // Illegal media type strings.  These will cause the MediaTypeHeaderValue ctor to throw FormatException
        public static readonly TestData<string> IllegalMediaTypeStrings = new RefTypeTestData<string>(() => new List<string>() 
        { 
            "\0",
            "9\r\n"
        });

        public static readonly TestData<Encoding> StandardEncodings = new RefTypeTestData<Encoding>(() => new List<Encoding>() 
        { 
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true),
            new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true),
        });

        public static IEnumerable<object[]> ReadAndWriteCorrectCharacterEncoding
        {
            get
            {
                yield return new object[] { "This is a test 激光這兩個字是甚麼意思 string written using utf-8", "utf-8", true };
                yield return new object[] { "This is a test 激光這兩個字是甚麼意思 string written using utf-16", "utf-16", true };
                yield return new object[] { "This is a test 激光這兩個字是甚麼意思 string written using utf-32", "utf-32", false };
                yield return new object[] { "This is a test 激光這兩個字是甚麼意思 string written using shift_jis", "shift_jis", false };
                yield return new object[] { "This is a test æøå string written using iso-8859-1", "iso-8859-1", false };
                yield return new object[] { "This is a test 레이저 단어 뜻 string written using iso-2022-kr", "iso-2022-kr", false };
            }
        }

        //// TODO: complete this list
        // Legal MediaTypeHeaderValues
        public static readonly TestData<MediaTypeHeaderValue> LegalMediaTypeHeaderValues = new RefTypeTestData<MediaTypeHeaderValue>(
            () => LegalMediaTypeStrings.Select<string, MediaTypeHeaderValue>((mediaType) => new MediaTypeHeaderValue(mediaType)).ToList());

        public static readonly TestData<MediaTypeWithQualityHeaderValue> StandardMediaTypesWithQuality = new RefTypeTestData<MediaTypeWithQualityHeaderValue>(() => new List<MediaTypeWithQualityHeaderValue>() 
        { 
            new MediaTypeWithQualityHeaderValue("application/json", .1) { CharSet="utf-8"},
            new MediaTypeWithQualityHeaderValue("text/json", .2) { CharSet="utf-8"},
            new MediaTypeWithQualityHeaderValue("application/xml", .3) { CharSet="utf-8"},
            new MediaTypeWithQualityHeaderValue("text/xml", .4) { CharSet="utf-8"},
            new MediaTypeWithQualityHeaderValue("application/atom+xml", .5) { CharSet="utf-8"},
        });

        public static readonly TestData<HttpContent> StandardHttpContents = new RefTypeTestData<HttpContent>(() => new List<HttpContent>() 
        { 
            new ByteArrayContent(new byte[0]),
            new FormUrlEncodedContent(new KeyValuePair<string, string>[0]),
            new MultipartContent(),
            new StringContent(""),
            new StreamContent(new MemoryStream())
        });

        //// TODO: make this list compose from other data?
        // Collection of legal instances of all standard MediaTypeMapping types
        public static readonly TestData<MediaTypeMapping> StandardMediaTypeMappings = new RefTypeTestData<MediaTypeMapping>(() =>
            QueryStringMappings.Cast<MediaTypeMapping>().Concat(
                    MediaRangeMappings.Cast<MediaTypeMapping>()).ToList()
        );

        public static readonly TestData<QueryStringMapping> QueryStringMappings = new RefTypeTestData<QueryStringMapping>(() => new List<QueryStringMapping>() 
        { 
            new QueryStringMapping("format", "json", new MediaTypeHeaderValue("application/json"))
        });

        public static readonly TestData<MediaRangeMapping> MediaRangeMappings = new RefTypeTestData<MediaRangeMapping>(() => new List<MediaRangeMapping>() 
        { 
            new MediaRangeMapping(new MediaTypeHeaderValue("application/*"), new MediaTypeHeaderValue("application/xml"))
        });

        public static readonly TestData<string> LegalUriPathExtensions = new RefTypeTestData<string>(() => new List<string>()
        { 
            "xml", 
            "json"
        });

        public static readonly TestData<string> LegalQueryStringParameterNames = new RefTypeTestData<string>(() => new List<string>()
        { 
            "format", 
            "fmt" 
        });

        public static readonly TestData<string> LegalHttpHeaderNames = new RefTypeTestData<string>(() => new List<string>()
        { 
            "x-requested-with", 
            "some-random-name" 
        });

        public static readonly TestData<string> LegalHttpHeaderValues = new RefTypeTestData<string>(() => new List<string>()
        { 
            "1", 
            "XMLHttpRequest",
            "\"quoted-string\""
        });

        public static readonly TestData<string> LegalQueryStringParameterValues = new RefTypeTestData<string>(() => new List<string>()
        { 
            "xml", 
            "json" 
        });

        public static readonly TestData<string> LegalMediaRangeStrings = new RefTypeTestData<string>(() => new List<string>()
        { 
            "application/*", 
            "text/*"
        });

        public static readonly TestData<MediaTypeHeaderValue> LegalMediaRangeValues = new RefTypeTestData<MediaTypeHeaderValue>(() =>
            LegalMediaRangeStrings.Select<string, MediaTypeHeaderValue>((s) => new MediaTypeHeaderValue(s)).ToList()
            );

        public static readonly TestData<MediaTypeWithQualityHeaderValue> MediaRangeValuesWithQuality = new RefTypeTestData<MediaTypeWithQualityHeaderValue>(() => new List<MediaTypeWithQualityHeaderValue>()
        {
            new MediaTypeWithQualityHeaderValue("application/*", .1),
            new MediaTypeWithQualityHeaderValue("text/*", .2),
        });

        public static readonly TestData<string> IllegalMediaRangeStrings = new RefTypeTestData<string>(() => new List<string>()
        { 
            "application/xml", 
            "text/xml" 
        });

        public static readonly TestData<MediaTypeHeaderValue> IllegalMediaRangeValues = new RefTypeTestData<MediaTypeHeaderValue>(() =>
            IllegalMediaRangeStrings.Select<string, MediaTypeHeaderValue>((s) => new MediaTypeHeaderValue(s)).ToList()
            );

        public static readonly TestData<MediaTypeFormatter> StandardFormatters = new RefTypeTestData<MediaTypeFormatter>(() => new List<MediaTypeFormatter>() 
        { 
            new XmlMediaTypeFormatter(),
            new JsonMediaTypeFormatter(),
            new FormUrlEncodedMediaTypeFormatter()
        });

        public static readonly TestData<Type> StandardFormatterTypes = new RefTypeTestData<Type>(() =>
            StandardFormatters.Select<MediaTypeFormatter, Type>((m) => m.GetType()));

        public static readonly TestData<MediaTypeFormatter> DerivedFormatters = new RefTypeTestData<MediaTypeFormatter>(() => new List<MediaTypeFormatter>() 
        { 
            new DerivedXmlMediaTypeFormatter(),
            new DerivedJsonMediaTypeFormatter(),
            new DerivedFormUrlEncodedMediaTypeFormatter(),
        });

        public static readonly TestData<IEnumerable<MediaTypeFormatter>> AllFormatterCollections =
            new RefTypeTestData<IEnumerable<MediaTypeFormatter>>(() => new List<IEnumerable<MediaTypeFormatter>>()
            {
                new MediaTypeFormatter[0],
                StandardFormatters,
                DerivedFormatters,
            });

        public static readonly TestData<string> LegalHttpAddresses = new RefTypeTestData<string>(() => new List<string>()
        { 
            "http://somehost", 
            "https://somehost",
        });

        public static readonly TestData<string> AddressesWithIllegalSchemes = new RefTypeTestData<string>(() => new List<string>()
        { 
            "net.tcp://somehost", 
            "file://somehost", 
            "net.pipe://somehost",
            "mailto:somehost",
            "ftp://somehost",
            "news://somehost",
            "ws://somehost",
            "abc://somehost"
        });

        /// <summary>
        /// A read-only collection of representative values and reference type test data.
        /// Uses where exhaustive coverage is not required.  It includes null values.
        /// </summary>
        public static readonly ReadOnlyCollection<TestData> RepresentativeValueAndRefTypeTestDataCollection = new ReadOnlyCollection<TestData>(new TestData[] {
             TestData.ByteTestData,
             TestData.IntTestData,
             TestData.BoolTestData,
             TestData.SimpleEnumTestData,
             TestData.StringTestData, 
             TestData.DateTimeTestData,
             TestData.DateTimeOffsetTestData,
             TestData.TimeSpanTestData,
             WcfPocoTypeTestDataWithNull
        });

        public static readonly TestData<HttpRequestMessage> NullContentHttpRequestMessages = new RefTypeTestData<HttpRequestMessage>(() => new List<HttpRequestMessage>()
        { 
           new HttpRequestMessage() { Content = null },
        });

        public static readonly TestData<string> LegalHttpParameterNames = new RefTypeTestData<string>(() => new List<string>()
        { 
            "文", 
            "A",
            "a",
            "b",
            " a",
            "arg1",
            "arg2",
            "1",
            "@",
            "!"

        });

        public static readonly TestData<Type> LegalHttpParameterTypes = new RefTypeTestData<Type>(() => new List<Type>()
        { 
            typeof(string),
            typeof(byte[]),
            typeof(byte[][]),
            typeof(byte[][]),
            typeof(char),
            typeof(DateTime),
            typeof(decimal),
            typeof(double),
            typeof(Guid),
            typeof(Int16),
            typeof(Int32),
            typeof(object),
            typeof(sbyte),
            typeof(Single),
            typeof(TimeSpan),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64),
            typeof(Uri),
            typeof(Enum),
            typeof(Collection<object>),
            typeof(IList<object>),
            typeof(System.Runtime.Serialization.ISerializable),
            typeof(System.Data.DataSet),
            typeof(System.Xml.Serialization.IXmlSerializable),
            typeof(Nullable),
            typeof(Nullable<DateTime>),
            typeof(Stream),
            typeof(HttpRequestMessage),
            typeof(HttpResponseMessage),
            typeof(ObjectContent),
            typeof(ObjectContent<object>),
            typeof(HttpContent),
            typeof(Delegate),
            typeof(Action),
            typeof(System.Threading.Tasks.Task<object>),
            typeof(System.Threading.Tasks.Task),
            typeof(List<dynamic>)
        });

        /// <summary>
        /// Common <see cref="TestData"/> for an <c>enum</c> decorated with a <see cref="DataContractAttribute"/>.
        /// </summary>
        public static readonly ValueTypeTestData<DataContractEnum> DataContractEnumTestData = new ValueTypeTestData<DataContractEnum>(
            DataContractEnum.First,
            DataContractEnum.Second);

        /// <summary>
        ///  Common <see cref="TestData"/> for the string form of a <see cref="Uri"/>.
        /// </summary>
        public static readonly RefTypeTestData<string> UriTestDataStrings = new RefTypeTestData<string>(() => new List<string>(){ 
            "http://somehost", 
            "http://somehost:8080", 
            "http://somehost/",
            "http://somehost:8080/", 
            "http://somehost/somepath", 
            "http://somehost/somepath/",
            "http://somehost/somepath?somequery=somevalue"});

        /// <summary>
        ///  Common <see cref="TestData"/> for a <see cref="Uri"/>.
        /// </summary>
        public static readonly RefTypeTestData<Uri> UriTestData = new RefTypeTestData<Uri>(() =>
            UriTestDataStrings.Select<string, Uri>((s) => new Uri(s)).ToList());

        /// <summary>
        ///  Common <see cref="TestData"/> for a POCO class type that includes null values
        ///  for both the base class and derived classes.
        /// </summary>
        public static readonly RefTypeTestData<WcfPocoType> WcfPocoTypeTestDataWithNull = new RefTypeTestData<WcfPocoType>(
            WcfPocoType.GetTestDataWithNull,
            WcfPocoType.GetDerivedTypeTestDataWithNull,
            null);
    }
}