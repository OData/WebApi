using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Routing;
using System.Web.OData.Routing.Conventions;
using Nuwa;
using Microsoft.OData.Edm;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Extensions;
using Newtonsoft.Json.Linq;

namespace WebStack.QA.Test.OData.Formatter
{
    [NuwaFramework]
    [NuwaHttpClientConfiguration(MessageLog = false)]
    public class ContextUriTests
    {
        [Theory]
        [InlineData("Businesses", "#Businesses")]
        [InlineData("Businesses(0)", "#Businesses/$entity")]
        [InlineData("Businesses(0)/Location", "#LocationSet")]
        [InlineData("unboundSetImport()", "#AppointmentSet")]
        [InlineData("unboundNoSetImport()", "#Collection(WebStack.QA.Test.OData.Formatter.Appointment)")]
        [InlineData("Businesses(0)/Appointments", "#Businesses(0)/Appointments")]
        [InlineData("Businesses(0)/Appointments('Appointment2')", "#Businesses(0)/Appointments/$entity")]
        [InlineData("Businesses(0)/Appointments('Appointment2')/Attendees", "#Collection(WebStack.QA.Test.OData.Formatter.Person)")]
        [InlineData("Businesses(0)/Manager/Appointments", "#Businesses(0)/Manager/Appointments")]
        [InlineData("Businesses(0)/Manager/Appointments('Appointment2')", "#Businesses(0)/Manager/Appointments/$entity")]
        [InlineData("Businesses(0)/Manager/Appointments('Appointment2')/Attendees", "#Collection(WebStack.QA.Test.OData.Formatter.Person)")]
        [InlineData("Businesses(0)/Default.boundEntity()", "#Businesses(0)/Appointments")]
        [InlineData("Businesses(0)/Default.boundNavPath()", "#Businesses(0)/Manager/Appointments")]
        [InlineData("Businesses(0)/Default.boundSet()", "#AppointmentSet")]
        [InlineData("Businesses(0)/Default.boundNoSet()", "#Collection(WebStack.QA.Test.OData.Formatter.Appointment)")]
        public void VerifyContextUrl(string query, string contextFragment)
        {
            string contextUrl = GetContextUrl(BaseAddress + "/odata/" + query);
            Assert.True(contextUrl.Contains(contextFragment), String.Format("Context URL for query {0} should contain {1} but is instead {2}", query, contextFragment, contextUrl));
        }

        private string GetContextUrl(string request)
        {
            HttpRequestMessage get = new HttpRequestMessage(HttpMethod.Get, request);
            get.Headers.Add("Accept", "application/json;odata.metadata=minimal");
            HttpResponseMessage response = Client.SendAsync(get).Result;
            Assert.True(response.IsSuccessStatusCode, String.Format("Error in Service: {0}", response.StatusCode));
            dynamic results = response.Content.ReadAsAsync<JObject>().Result;
            return results["@odata.context"].Value as string;
        }

        #region Nuwa

        private string baseAddress = null;

        [NuwaBaseAddress]
        public string BaseAddress
        {
            get
            {
                return baseAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.baseAddress = value.Replace("localhost", Environment.MachineName);
                }
            }
        }

        [NuwaHttpClient]
        public HttpClient Client { get; set; }

        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration config)
        {
            config.Routes.Clear();
            config.Services.Replace(
                typeof(System.Web.Http.Dispatcher.IAssembliesResolver),
                new Common.TestAssemblyResolver(typeof(ContextUrlTestController), typeof(MetadataController)));
            config.MapODataServiceRoute("odata", "odata", GetModel(), new DefaultODataPathHandler(), ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", config));
        }

        #endregion Nuwa

        #region Model
        private static IEdmModel GetModel()
        {
            ODataModelBuilder builder = new ODataConventionModelBuilder();
            builder.Namespace = "Default";

            var appointmentType = builder.EntityType<Appointment>();
            var businessType = builder.EntityType<Business>();
            var managerType = builder.EntityType<Manager>();
            var peopleType = builder.EntityType<Person>();
            var locationType = builder.EntityType<Location>();

            var function1 = businessType.Function("boundEntity");
            function1.ReturnsCollectionViaEntitySetPath<Appointment>("bindingParameter/Appointments");
            function1.OptionalReturn = false;

            var function2 = businessType.Function("boundNavPath");
            function2.ReturnsCollectionViaEntitySetPath<Appointment>("bindingParameter/Manager/Appointments");
            function2.OptionalReturn = false;

            var function3 = businessType.Function("boundSet");
            function3.ReturnsCollectionFromEntitySet<Appointment>("AppointmentSet");
            function3.OptionalReturn = false;

            var function4 = businessType.Function("boundNoSet");
            function4.ReturnsCollection<Appointment>();
            function4.OptionalReturn = false;

            var function5 = builder.Function("unboundSet");
            function5.ReturnsCollectionFromEntitySet<Appointment>("AppointmentSet");
            function5.OptionalReturn = false;

            var function6 = builder.Function("unboundNoSet");
            function6.ReturnsCollection<Appointment>();
            function6.OptionalReturn = false;

            var appointmentSet = builder.EntitySet<Appointment>("AppointmentSet");
            var locationSet = builder.EntitySet<Location>("LocationSet");
            var businessSet = builder.EntitySet<Business>("Businesses");
            businessSet.HasRequiredBinding(b => b.Location, locationSet);
            var personSet = builder.EntitySet<Person>("PersonSet");
            appointmentSet.HasManyBinding(a => a.Attendees, personSet);
            
            IEdmModel edmModel = builder.GetEdmModel();
            var container = edmModel.EntityContainer as EdmEntityContainer;
            var unboundSet = edmModel.FindOperations("Default.unboundSet").FirstOrDefault() as IEdmFunction;
            var unboundNoSet = edmModel.FindOperations("Default.unboundNoSet").FirstOrDefault() as IEdmFunction;
            container.AddFunctionImport("unboundSetImport", unboundSet, new EdmPathExpression("AppointmentSet")); 
            container.AddFunctionImport("unboundNoSetImport", unboundNoSet);

            return edmModel;
        }
    }

    #endregion Model

    #region model classes

    public class Business
    {
        [Key]
        public int Id
        {
            get;
            set;
        }

        [Contained]
        public Manager Manager
        {
            get;
            set;
        }

        [Contained]
        [ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public Appointment[] Appointments
        {
            get;
            set;
        }

        public Location Location
        {
            get;
            set;
        }
    }

    public class Location
    {
        [Key]
        public string Name
        {
            get;
            set;
        }
    }

    public class Manager
    {
        [Key]
        public int Id
        {
            get;
            set;
        }

        [Contained]
        public Appointment[] Appointments
        {
            get;
            set;
        }
    }

    public class Appointment
    {
        [Key]
        public string Id
        {
            get;
            set;
        }

        public Person[] Attendees
        {
            get;
            set;
        }
    }

    public class Person
    {
        [Key]
        public string FullName
        {
            get;
            set;
        }
    }

    #endregion model classes

    #region Controllers
    public class ContextUrlTestController : ODataController
    {

        #region business methods

        [EnableQuery]
        [ODataRoute("Businesses")]
        public IQueryable<Business> Get()
        {
            return Enumerable.Range(0, 5).Select(i =>
                   new Business
                   {
                       Id = i,
                   }).AsQueryable();
        }
        
        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})")]
        public SingleResult<Business> GetBusiness([FromODataUri]int BusinessId)
        {
            var business = new Business { Id = BusinessId };
            return business.AsSingleResult();
        }

        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Location")]
        public SingleResult<Location> GetBusinessLocation([FromODataUri]int BusinessId)
        {
            var location = new Location { Name = "School" };
            return location.AsSingleResult();
        }
        #endregion

        #region navigation methods

        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Appointments")]
        public IQueryable<Appointment> GetBusinessAppointments([FromODataUri]int BusinessId)
        {
            return
                Enumerable.Range(0, 5).Select(i =>
                    new Appointment
                    {
                        Id = BusinessId + "." + i.ToString(),
                    }).AsQueryable();
        }


        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Appointments({AppointmentId})")]
        public SingleResult<Appointment> GetBusinessAppointment([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        {
            var appointment = new Appointment { Id = AppointmentId };
            return appointment.AsSingleResult();
        }

        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Appointments({AppointmentId})/Attendees")]
        public IQueryable<Person> GetBusinessAppointmentAttendees([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        {
            return getAttendees();
        }

        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Manager/Appointments")]
        public IQueryable<Appointment> GetBusinessManagerAppointments([FromODataUri]int BusinessId)
        {
            return Enumerable.Range(0, 5).Select(i =>
                     new Appointment
                     {
                         Id = BusinessId + "." + i.ToString(),
                     }).AsQueryable();
        }

        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Manager/Appointments({AppointmentId})")]
        public SingleResult<Appointment> GetBusinessManagerAppointment([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        {
            var appointment = new Appointment { Id = AppointmentId };
            return appointment.AsSingleResult();
        }

        [EnableQuery]
        [ODataRoute("Businesses({BusinessId})/Manager/Appointments({AppointmentId})/Attendees")]
        public IQueryable<Person> GetBusinessManagerAppointmentAttendees([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        {
            return getAttendees();
        }
        
        #endregion

        #region functions

        [HttpGet]
        [ODataRoute("Businesses({BusinessId})/Default.boundEntity()")]
        public IHttpActionResult boundEntity([FromODataUri]int BusinessId)
        {
            return Ok(getAppointments());
        }

        //[HttpGet]
        //[EnableQuery]
        //[ODataRoute("Businesses({BusinessId})/Default.boundEntity()({AppointmentId})")]
        //public SingleResult<Appointment> boundEntityAppointment([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return getAppointments().Where(a => a.Id == AppointmentId).FirstOrDefault().AsSingleResult();
        //}

        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundEntity()({AppointmentId})/Attendees")]
        //public IQueryable<Person> boundEntityAppointmentLocation([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return getAttendees();
        //}

        [HttpGet]
        [ODataRoute("Businesses({BusinessId})/Default.boundNavPath()")]
        public IHttpActionResult boundNavPath([FromODataUri]int BusinessId)
        {
            return Ok(getAppointments());
        }

        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundNavPath()({AppointmentId})")]
        //public IHttpActionResult boundNavPathAppointment([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return Ok(getAppointments().Where(a => a.Id == AppointmentId));
        //}

        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundNavPath()({AppointmentId})/Attendees")]
        //public IQueryable<Person> boundNavPathAppointmentAttendees([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return getAttendees();
        //}

        [HttpGet]
        [ODataRoute("Businesses({BusinessId})/Default.boundSet()")]
        public IHttpActionResult boundSet([FromODataUri]int BusinessId)
        {
            return Ok(getAppointments());
        }
        
        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundSet()({AppointmentId})")]
        //public IHttpActionResult boundSetAppointment([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return Ok(getAppointments().Where(a => a.Id == AppointmentId));
        //}

        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundSet()({AppointmentId})/Location")]
        //public IQueryable<Person> boundSetAppointmentAttendees([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return getAttendees();
        //}

        [HttpGet]
        [ODataRoute("Businesses({BusinessId})/Default.boundNoSet()")]
        public IHttpActionResult boundNoSet([FromODataUri]int BusinessId)
        {
            return Ok(getAppointments());
        }

        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundNoSet()({AppointmentId})")]
        //public IHttpActionResult boundNoSetAppointment([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return Ok(getAppointments().Where(a => a.Id == AppointmentId));
        //}

        //[HttpGet]
        //[ODataRoute("Businesses({BusinessId})/Default.boundNoSet()({AppointmentId})/Location")]
        //public IQueryable<Person> boundNoSetAppointmentAttendees([FromODataUri]int BusinessId, [FromODataUri]string AppointmentId)
        //{
        //    return getAttendees();
        //}

        #endregion Functions

        #region FunctionImports

        [HttpGet]
        [EnableQuery]
        [ODataRoute("unboundSetImport()")]
        public IQueryable<Appointment> unboundSetImport()
        {
            return getAppointments();
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("unboundNoSetImport()")]
        public IQueryable<Appointment> unboundNoSetImport()
        {
            return getAppointments();
        }


        #endregion FunctionImports


        private IQueryable<Appointment> getAppointments()
        {
            return Enumerable.Range(2, 4).Select(i =>
                 new Appointment
                 {
                     Id = "Appointment" + i.ToString(),
                 }).AsQueryable();
        }

        private IQueryable<Person> getAttendees()
        {
            return Enumerable.Range(1, 3).Select(i =>
            new Person
            {
                FullName = "Person" + i
            }).AsQueryable();
        }
    }


    #endregion Controller

    public static class ExtensionMethods
    {
        public static SingleResult<T> AsSingleResult<T>(this T value)
        {
            return new[] { value }.AsSingleResult();
        }

        public static SingleResult<T> AsSingleResult<T>(this T[] values)
        {
            return new SingleResult<T>(values.AsQueryable());
        }
    }
}