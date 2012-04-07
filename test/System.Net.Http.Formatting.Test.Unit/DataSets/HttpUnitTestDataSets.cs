// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Formatting.DataSets.Types;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting.DataSets
{
    public class HttpUnitTestDataSets
    {
        public static TestData<HttpMethod> AllHttpMethods { get { return HttpTestData.AllHttpMethods; } }

        public static TestData<HttpMethod> StandardHttpMethods { get { return HttpTestData.StandardHttpMethods; } }

        public static TestData<HttpMethod> CustomHttpMethods { get { return HttpTestData.CustomHttpMethods; } }

        public static TestData<HttpStatusCode> AllHttpStatusCodes { get { return HttpTestData.AllHttpStatusCodes; } }

        public static TestData<HttpStatusCode> CustomHttpStatusCodes { get { return HttpTestData.CustomHttpStatusCodes; } }

        public static ReadOnlyCollection<TestData> ConvertablePrimitiveValueTypes { get { return HttpTestData.ConvertablePrimitiveValueTypes; } }

        public static ReadOnlyCollection<TestData> ConvertableEnumTypes { get { return HttpTestData.ConvertableEnumTypes; } }

        public static ReadOnlyCollection<TestData> ConvertableValueTypes { get { return HttpTestData.ConvertableValueTypes; } }

        public static TestData<MediaTypeHeaderValue> StandardJsonMediaTypes { get { return HttpTestData.StandardJsonMediaTypes; } }

        public static TestData<MediaTypeHeaderValue> StandardXmlMediaTypes { get { return HttpTestData.StandardXmlMediaTypes; } }

        public static TestData<MediaTypeHeaderValue> StandardODataMediaTypes { get { return HttpTestData.StandardODataMediaTypes; } }

        public static TestData<MediaTypeHeaderValue> StandardFormUrlEncodedMediaTypes { get { return HttpTestData.StandardFormUrlEncodedMediaTypes; } }

        public static TestData<MediaTypeWithQualityHeaderValue> StandardMediaTypesWithQuality { get { return HttpTestData.StandardMediaTypesWithQuality; } }

        public static TestData<string> StandardJsonMediaTypeStrings { get { return HttpTestData.StandardXmlMediaTypeStrings; } }

        public static TestData<string> StandardXmlMediaTypeStrings { get { return HttpTestData.StandardXmlMediaTypeStrings; } }

        public static TestData<string> LegalMediaTypeStrings { get { return HttpTestData.LegalMediaTypeStrings; } }

        public static TestData<string> IllegalMediaTypeStrings { get { return HttpTestData.IllegalMediaTypeStrings; } }

        public static TestData<Encoding> StandardEncodings { get { return HttpTestData.StandardEncodings; } }

        public static TestData<MediaTypeHeaderValue> LegalMediaTypeHeaderValues { get { return HttpTestData.LegalMediaTypeHeaderValues; } }

        public static TestData<HttpContent> StandardHttpContents { get { return HttpTestData.StandardHttpContents; } }

        public static TestData<MediaTypeMapping> StandardMediaTypeMappings { get { return HttpTestData.StandardMediaTypeMappings; } }

        public static TestData<QueryStringMapping> QueryStringMappings { get { return HttpTestData.QueryStringMappings; } }

        public static TestData<MediaRangeMapping> MediaRangeMappings { get { return HttpTestData.MediaRangeMappings; } }

        public static TestData<string> LegalUriPathExtensions { get { return HttpTestData.LegalUriPathExtensions; } }

        public static TestData<string> LegalQueryStringParameterNames { get { return HttpTestData.LegalQueryStringParameterNames; } }

        public static TestData<string> LegalQueryStringParameterValues { get { return HttpTestData.LegalQueryStringParameterValues; } }

        public static TestData<string> LegalHttpHeaderNames { get { return HttpTestData.LegalHttpHeaderNames; } }

        public static TestData<string> LegalHttpHeaderValues { get { return HttpTestData.LegalHttpHeaderValues; } }

        public static TestData<string> LegalMediaRangeStrings { get { return HttpTestData.LegalMediaRangeStrings; } }

        public static TestData<MediaTypeHeaderValue> LegalMediaRangeValues { get { return HttpTestData.LegalMediaRangeValues; } }

        public static TestData<MediaTypeWithQualityHeaderValue> MediaRangeValuesWithQuality { get { return HttpTestData.MediaRangeValuesWithQuality; } }

        public static TestData<string> IllegalMediaRangeStrings { get { return HttpTestData.IllegalMediaRangeStrings; } }

        public static TestData<MediaTypeHeaderValue> IllegalMediaRangeValues { get { return HttpTestData.IllegalMediaRangeValues; } }

        public static TestData<MediaTypeFormatter> StandardFormatters { get { return HttpTestData.StandardFormatters; } }

        public static TestData<Type> StandardFormatterTypes { get { return HttpTestData.StandardFormatterTypes; } }

        public static TestData<MediaTypeFormatter> DerivedFormatters { get { return HttpTestData.DerivedFormatters; } }

        public static TestData<IEnumerable<MediaTypeFormatter>> AllFormatterCollections { get { return HttpTestData.AllFormatterCollections; } }

        public static TestData<string> LegalHttpAddresses { get { return HttpTestData.LegalHttpAddresses; } }

        public static TestData<string> AddressesWithIllegalSchemes { get { return HttpTestData.AddressesWithIllegalSchemes; } }

        public static TestData<HttpRequestMessage> NullContentHttpRequestMessages { get { return HttpTestData.NullContentHttpRequestMessages; } }

        public static ReadOnlyCollection<TestData> RepresentativeValueAndRefTypeTestDataCollection { get { return HttpTestData.RepresentativeValueAndRefTypeTestDataCollection; } }

        public static TestData<string> LegalHttpParameterNames { get { return HttpTestData.LegalHttpParameterNames; } }

        public static TestData<Type> LegalHttpParameterTypes { get { return HttpTestData.LegalHttpParameterTypes; } }

        public static RefTypeTestData<Uri> Uris { get { return HttpTestData.UriTestData; } }

        public static RefTypeTestData<string> UriStrings { get { return HttpTestData.UriTestDataStrings; } }

        public static RefTypeTestData<WcfPocoType> PocoTypesWithNull { get { return HttpTestData.WcfPocoTypeTestDataWithNull; } }
    }
}
