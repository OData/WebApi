﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Enums
{
    internal class EnumsEdmModel
    {
        public static IEdmModel GetExplicitModel()
        {
            ODataModelBuilder builder = new ODataModelBuilder();
            var employee = builder.EntityType<Employee>();
            employee.HasKey(c => c.ID);
            employee.Property(c => c.Name);
            employee.CollectionProperty<Skill>(c => c.SkillSet);
            employee.EnumProperty<Gender>(c => c.Gender);
            employee.EnumProperty<AccessLevel>(c => c.AccessLevel);
            employee.ComplexProperty<FavoriteSports>(c => c.FavoriteSports);

            var skill = builder.EnumType<Skill>();
            skill.Member(Skill.CSharp);
            skill.Member(Skill.Sql);
            skill.Member(Skill.Web);

            var gender = builder.EnumType<Gender>();
            gender.Member(Gender.Female);
            gender.Member(Gender.Male);

            var accessLevel = builder.EnumType<AccessLevel>();
            accessLevel.Member(AccessLevel.Execute);
            accessLevel.Member(AccessLevel.Read);
            accessLevel.Member(AccessLevel.Write);

            var favoriteSports = builder.ComplexType<FavoriteSports>();
            favoriteSports.EnumProperty<Sport>(f => f.LikeMost);
            favoriteSports.CollectionProperty<Sport>(f => f.Like);

            var sport = builder.EnumType<Sport>();
            sport.Member(Sport.Basketball);
            sport.Member(Sport.Pingpong);

            AddBoundActionsAndFunctions(employee);
            AddUnboundActionsAndFunctions(builder);

            EntitySetConfiguration<Employee> employees = builder.EntitySet<Employee>("Employees");
            builder.Namespace = typeof(Employee).Namespace;
            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Employee> employees = builder.EntitySet<Employee>("Employees");
            EntityTypeConfiguration<Employee> employee = employees.EntityType;

            // maybe following lines are not required once bug #1587 is fixed.
            // 1587: It's better to support automatically adding actions and functions in ODataConventionModelBuilder.
            AddBoundActionsAndFunctions(employee);
            AddUnboundActionsAndFunctions(builder);

            builder.Namespace = typeof(Employee).Namespace;

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
