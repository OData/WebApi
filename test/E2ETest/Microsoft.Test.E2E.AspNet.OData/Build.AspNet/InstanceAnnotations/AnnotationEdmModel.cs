// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using Microsoft.Test.E2E.AspNet.OData.Common.Execution;
using NS;

namespace Microsoft.Test.E2E.AspNet.OData.InstanceAnnotations
{
    internal class AnnotationEdmModel
    {
        public static IEdmModel GetExplicitModel(WebRouteConfiguration configuration)
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var employee = builder.EntityType<Employee>();
            employee.HasKey(c => c.ID);
            employee.Property(c => c.Name);
            employee.CollectionProperty<Skill>(c => c.SkillSet);
            employee.EnumProperty<Gender>(c => c.Gender);
            employee.EnumProperty<AccessLevel>(c => c.AccessLevel);
            employee.ComplexProperty<FavoriteSports>(c => c.FavoriteSports);
            employee.Namespace = "NS";

            var skill = builder.EnumType<Skill>();
            skill.Member(Skill.CSharp);
            skill.Member(Skill.Sql);
            skill.Member(Skill.Web);
            skill.Namespace = "NS";

            var gender = builder.EnumType<Gender>();
            gender.Member(Gender.Female);
            gender.Member(Gender.Male);
            gender.Namespace = "NS";

            var accessLevel = builder.EnumType<AccessLevel>();
            accessLevel.Member(AccessLevel.Execute);
            accessLevel.Member(AccessLevel.Read);
            accessLevel.Member(AccessLevel.Write);
            accessLevel.Namespace = "NS";

            var favoriteSports = builder.ComplexType<FavoriteSports>();
            favoriteSports.EnumProperty<Sport>(f => f.LikeMost);
            favoriteSports.CollectionProperty<Sport>(f => f.Like);
            favoriteSports.Namespace = "NS";

            var sport = builder.EnumType<Sport>();
            sport.Member(Sport.Basketball);
            sport.Member(Sport.Pingpong);
            sport.Namespace = "NS";

            AddBoundActionsAndFunctions(employee);
            AddUnboundActionsAndFunctions(builder);

            EntitySetConfiguration<Employee> employees = builder.EntitySet<Employee>("Employees");
            builder.Namespace = "NS" ;
            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel(WebRouteConfiguration configuration)
        {
            ODataConventionModelBuilder builder = configuration.CreateConventionModelBuilder();
            EntitySetConfiguration<Employee> employees = builder.EntitySet<Employee>("Employees");
            EntityTypeConfiguration<Employee> employee = employees.EntityType;

           AddBoundActionsAndFunctions(employee);
            AddUnboundActionsAndFunctions(builder);

            builder.Namespace = "NS";

            var edmModel = builder.GetEdmModel();
            return edmModel;
        }

        private static void AddBoundActionsAndFunctions(EntityTypeConfiguration<Employee> employee)
        {
            var actionConfiguration = employee.Action("AddSkill");
            actionConfiguration.Parameter<Skill>("skill");
            actionConfiguration.ReturnsCollection<Skill>();

            var functionConfiguration = employee.Function("GetAccessLevel");
            functionConfiguration.Returns<AccessLevel>();
        }

        private static void AddUnboundActionsAndFunctions(ODataModelBuilder odataModelBuilder)
        {
            var actionConfiguration = odataModelBuilder.Action("SetAccessLevel");
            actionConfiguration.Parameter<int>("ID");
            actionConfiguration.Parameter<AccessLevel>("accessLevel");
            actionConfiguration.Returns<AccessLevel>();

            var functionConfiguration = odataModelBuilder.Function("HasAccessLevel");
            functionConfiguration.Parameter<int>("ID");
            functionConfiguration.Parameter<AccessLevel>("AccessLevel");
            functionConfiguration.Returns<bool>();

            var actionConfiguration2 = odataModelBuilder.Action("ResetDataSource");
        }
    }
}
