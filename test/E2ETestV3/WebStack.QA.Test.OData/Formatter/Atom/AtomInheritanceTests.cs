using System;
using System.Web.Http;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Routing;
using System.Web.Http.OData.Routing.Conventions;
using Nuwa;
using WebStack.QA.Common.WebHost;
using WebStack.QA.Test.OData.Common;
using WebStack.QA.Test.OData.Common.Models.Vehicle;
using Xunit;
using Xunit.Extensions;

namespace WebStack.QA.Test.OData.Formatter.Atom
{
    public class AtomInheritanceTests : InheritanceTests
    {
        [NuwaConfiguration]
        public static void UpdateConfiguration(HttpConfiguration configuration)
        {
            configuration.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

            var conventions = ODataRoutingConventions.CreateDefault();
            conventions.Insert(0, new NavigationRoutingConvention2());
            conventions.Insert(0, new LinkRoutingConvention2());

            configuration.Routes.MapODataServiceRoute(
                ODataTestConstants.DefaultRouteName, 
                null, 
                GetEdmModel(configuration), 
                new DefaultODataPathHandler(), 
                conventions);

            configuration.AddODataQueryFilter();
        }

        [NuwaWebConfig]
        public static void UpdateWebConfig(WebConfigHelper config)
        {
            config.AddODataLibAssemblyRedirection();
        }

        [Theory]
        [InlineData(typeof(Car), "InheritanceTests_MovingObjects")]
        [InlineData(typeof(Vehicle), "InheritanceTests_MovingObjects")]
        [InlineData(typeof(Vehicle), "InheritanceTests_Vehicles")]
        [InlineData(typeof(Car), "InheritanceTests_Cars")]
        [InlineData(typeof(Car), "InheritanceTests_Vehicles")]
        [InlineData(typeof(SportBike), "InheritanceTests_Vehicles")]
        public void PostGetUpdateAndDeleteAtom(Type entityType, string entitySetName)
        {
            PostGetUpdateAndDelete(entityType, entitySetName);
        }

        [Fact]
        public void AddAndRemoveBaseNavigationPropertyInDerivedTypeAtom()
        {
            AddAndRemoveBaseNavigationPropertyInDerivedType();
        }

        [Fact]
        public void AddAndRemoveDerivedNavigationPropertyInDerivedTypeAtom()
        {
            AddAndRemoveDerivedNavigationPropertyInDerivedType();
        }

        [Fact]
        public override void CreateAndDeleteLinkToDerivedNavigationPropertyOnBaseEntitySet()
        {
            base.CreateAndDeleteLinkToDerivedNavigationPropertyOnBaseEntitySet();
        }

        [Theory]
        [InlineData("/InheritanceTests_Vehicles(1)/WebStack.QA.Test.OData.Common.Models.Vehicle.Car/Wash")]
        [InlineData("/InheritanceTests_Vehicles(1)/WebStack.QA.Test.OData.Common.Models.Vehicle.SportBike/Wash")]
        public override void InvokeActionWithOverloads(string actionUrl)
        {
            base.InvokeActionWithOverloads(actionUrl);
        }
    }
}
