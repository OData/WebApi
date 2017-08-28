// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.Test.AspNet.OData.TestCommon;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.AspNet.OData
{
    public class DateAndTimeOfDayTest
    {
        [Fact]
        public void MetadataDocument_IncludesDateAndTimeOfDayProperties_FromDateTimeAndTimeSpan()
        {
            // Arrange
            const string Uri = "http://localhost/odata/$metadata";
            HttpClient client = GetClient();

            // Act
            HttpResponseMessage response = client.GetAsync(Uri).Result;
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            Assert.Equal("application/xml", response.Content.Headers.ContentType.MediaType);

            string payload = response.Content.ReadAsStringAsync().Result;
            Assert.Contains("<Property Name=\"Birthday\" Type=\"Edm.Date\" Nullable=\"false\" />", payload);
            Assert.Contains("<Property Name=\"PublishDay\" Type=\"Edm.Date\" />", payload);
            Assert.Contains("<Property Name=\"CreatedTime\" Type=\"Edm.TimeOfDay\" Nullable=\"false\" />", payload);
            Assert.Contains("<Property Name=\"EdmTime\" Type=\"Edm.TimeOfDay\" />", payload);
            Assert.Contains("<Property Name=\"ResumeTime\" Type=\"Edm.Duration\" Nullable=\"false\" />", payload);
        }

        [Fact]
        public void CanQueryEntity_WithDateAndTimeOfDayProperties()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DateAndTimeOfDayModels(2)";
            HttpClient client = GetClient();

            // Act
            HttpResponseMessage response = client.GetAsync(Uri).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal("http://localhost/odata/$metadata#DateAndTimeOfDayModels/$entity", result["@odata.context"]);
            Assert.Equal(2, result["Id"]);
            Assert.Equal("2016-12-31", result["Birthday"]);
            Assert.Equal(JValue.CreateNull(), result["PublishDay"]);
            Assert.Equal("02:04:05.1980000", result["CreatedTime"]);
            Assert.Equal("08:02:05.1980000", result["EdmTime"]);
            Assert.Equal("PT2H4M5.198S", result["ResumeTime"]);
        }

        [Fact]
        public void CanQueryOption_OnDateAndTimeOfDayProperties()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DateAndTimeOfDayModels?$filter=Birthday eq 2017-12-31&$select=Birthday,CreatedTime";
            const string Expect = @"{
  ""@odata.context"": ""http://localhost/odata/$metadata#DateAndTimeOfDayModels(Birthday,CreatedTime)"",
  ""value"": [
    {
      ""Birthday"": ""2017-12-31"",
      ""CreatedTime"": ""03:04:05.1980000""
    }
  ]
}";

            HttpClient client = GetClient();

            // Act
            HttpResponseMessage response = client.GetAsync(Uri).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(Expect, result.ToString());
        }

        [Fact]
        public void CanFilter_OnTimeOfDayProperties()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DateAndTimeOfDayModels?$filter=EdmTime eq 08:02:05.1980000&$select=EdmTime,ResumeTime";
            const string Expect = @"{
  ""@odata.context"": ""http://localhost/odata/$metadata#DateAndTimeOfDayModels(EdmTime,ResumeTime)"",
  ""value"": [
    {
      ""EdmTime"": ""08:02:05.1980000"",
      ""ResumeTime"": ""PT2H4M5.198S""
    }
  ]
}";

            HttpClient client = GetClient();

            // Act
            HttpResponseMessage response = client.GetAsync(Uri).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);
            Assert.Equal(Expect, result.ToString());
        }

        [Fact]
        public void CanOrderBy_OnDateTimeProperty()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DateAndTimeOfDayModels?$orderby=Birthday desc";
            HttpClient client = GetClient();

            // Act
            HttpResponseMessage response = client.GetAsync(Uri).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);

            var result = JObject.Parse(response.Content.ReadAsStringAsync().Result);

            Assert.Equal(5, result["value"].Count());
            Assert.Equal("2019-12-31", result["value"][0]["Birthday"]);
            Assert.Equal("2018-12-31", result["value"][1]["Birthday"]);
            Assert.Equal("2017-12-31", result["value"][2]["Birthday"]);
            Assert.Equal("2016-12-31", result["value"][3]["Birthday"]);
            Assert.Equal("2015-12-31", result["value"][4]["Birthday"]);
        }

        [Fact]
        public void PostEntity_WithDateAndTimeOfDayProperties()
        {
            // Arrange
            const string Payload = "{" +
                "\"Id\":99," +
                "\"Birthday\":\"2099-01-01\"," +
                "\"CreatedTime\":\"13:14:15.2190000\"" +
                "}";
            const string Uri = "http://localhost/odata/DateAndTimeOfDayModels";
            HttpClient client = GetClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Uri);

            request.Content = new StringContent(Payload);
            request.Content.Headers.ContentType = MediaTypeWithQualityHeaderValue.Parse("application/json");

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(new Uri("http://localhost/odata/DateAndTimeOfDayModels(99)"), response.Headers.Location);
        }

        [Fact]
        public void CanQuerySingleDateTimeProperty()
        {
            // Arrange
            const string Uri = "http://localhost/odata/DateAndTimeOfDayModels(3)/Birthday";
            const string Expect = @"{""@odata.context"":""http://localhost/odata/$metadata#DateAndTimeOfDayModels(3)/Birthday"",""value"":""2017-12-31""}";

            HttpClient client = GetClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Uri);

            // Act
            HttpResponseMessage response = client.SendAsync(request).Result;

            // Assert
            Assert.True(response.IsSuccessStatusCode);
            Assert.Equal(Expect, response.Content.ReadAsStringAsync().Result);
        }

        private static HttpClient GetClient()
        {
            HttpConfiguration config =
                new[] { typeof(MetadataController), typeof(DateAndTimeOfDayModelsController) }.GetHttpConfiguration();
            config.Count().OrderBy().Filter().Expand().MaxTop(null).Select();
            config.MapODataServiceRoute("odata", "odata", GetEdmModel());
            return new HttpClient(new HttpServer(config));
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<DateAndTimeOfDayModel>("DateAndTimeOfDayModels");
            return builder.GetEdmModel();
        }
    }

    public class DateAndTimeOfDayModelsController : ODataController
    {
        private DateAndTimeOfDayModelContext db = new DateAndTimeOfDayModelContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(db.DateTimes);
        }

        [EnableQuery]
        public IHttpActionResult Get(int key)
        {
            DateAndTimeOfDayModel dtm = db.DateTimes.FirstOrDefault(e => e.Id == key);
            if (dtm == null)
            {
                return NotFound();
            }

            return Ok(dtm);
        }

        public IHttpActionResult Post([FromBody]DateAndTimeOfDayModel dt)
        {
            Assert.NotNull(dt);

            Assert.Equal(99, dt.Id);
            Assert.Equal(new DateTime(2099, 1, 1), dt.Birthday);
            Assert.Equal(new TimeSpan(0, 13, 14, 15, 219), dt.CreatedTime);

            return Created(dt);
        }

        public IHttpActionResult GetBirthday(int key)
        {
            DateAndTimeOfDayModel dtm = db.DateTimes.FirstOrDefault(e => e.Id == key);
            if (dtm == null)
            {
                return NotFound();
            }

            Date dt = dtm.Birthday;
            return Ok(dt);
        }
    }

    class DateAndTimeOfDayModelContext
    {
        private static IList<DateAndTimeOfDayModel> _dateTimes;

        static DateAndTimeOfDayModelContext()
        {
            DateTime dt = new DateTime(2014, 12, 31);

            _dateTimes = Enumerable.Range(1, 5).Select(i =>
                new DateAndTimeOfDayModel
                {
                    Id = i,
                    Birthday = dt.AddYears(i),
                    PublishDay = i % 2 == 0 ? (DateTime?)null : dt.AddMonths(i),
                    CreatedTime = new TimeSpan(0, i, 4, 5, 198),
                    EdmTime = i % 3 == 0 ? (TimeSpan?)null : new TimeSpan(0, 8, i, 5, 198),
                    ResumeTime = new TimeSpan(0, i, 4, 5, 198)
                }).ToList();
        }

        public IEnumerable<DateAndTimeOfDayModel> DateTimes { get { return _dateTimes; } }
    }

    public class DateAndTimeOfDayModel
    {
        public int Id { get; set; }

        [Column(TypeName = "date")]
        public DateTime Birthday { get; set; }

        [Column(TypeName = "date")]
        public DateTime? PublishDay { get; set; }

        [Column(TypeName = "time")]
        public TimeSpan CreatedTime { get; set; }

        [Column(TypeName = "tIme")]
        public TimeSpan? EdmTime { get; set; }

        public TimeSpan ResumeTime { get; set; }
    }
}
