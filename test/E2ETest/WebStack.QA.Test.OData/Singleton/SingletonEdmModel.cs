﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Singleton
{
    internal class SingletonEdmModel
    {
        public static IEdmModel GetExplicitModel(string singletonName)
        {
            ODataModelBuilder builder = new ODataModelBuilder();

            // Define EntityType of Partner
            var partner = builder.EntityType<Partner>();
            partner.HasKey(p => p.ID);
            partner.Property(p => p.Name);
            var partnerCompany = partner.HasRequired(p => p.Company);

            // Define Enum Type
            var category = builder.EnumType<CompanyCategory>();
            category.Member(CompanyCategory.IT);
            category.Member(CompanyCategory.Communication);
            category.Member(CompanyCategory.Electronics);
            category.Member(CompanyCategory.Others);

            // Define EntityType of Company
            var company = builder.EntityType<Company>();
            company.HasKey(p => p.ID);
            company.Property(p => p.Name);
            company.Property(p => p.Revenue);
            company.EnumProperty(p => p.Category);
            var companyPartners = company.HasMany(p => p.Partners);
            companyPartners.IsNotCountable();

            var companyBranches = company.CollectionProperty(p => p.Branches);

            // Define Complex Type: Office
            var office = builder.ComplexType<Office>();
            office.Property(p => p.City);
            office.Property(p => p.Address);

            // Define Derived Type: SubCompany
            var subCompany = builder.EntityType<SubCompany>();
            subCompany.DerivesFrom<Company>();
            subCompany.Property(p => p.Location);
            subCompany.Property(p => p.Description);
            subCompany.ComplexProperty(p => p.Office);

            builder.Namespace = typeof(Partner).Namespace;

            // Define PartnerSet and Company(singleton)
            EntitySetConfiguration<Partner> partnersConfiguration = builder.EntitySet<Partner>("Partners");
            partnersConfiguration.HasIdLink(c=>c.GenerateSelfLink(false), true);
            partnersConfiguration.HasSingletonBinding(c => c.Company, singletonName);
            Func<ResourceContext<Partner>, IEdmNavigationProperty, Uri> link = (eic, np) => eic.GenerateNavigationPropertyLink(np, false);
            partnersConfiguration.HasNavigationPropertyLink(partnerCompany, link, true);
            partnersConfiguration.EntityType.Collection.Action("ResetDataSource");

            SingletonConfiguration<Company> companyConfiguration = builder.Singleton<Company>(singletonName);
            companyConfiguration.HasIdLink(c => c.GenerateSelfLink(false), true);
            companyConfiguration.HasManyBinding(c => c.Partners, "Partners");
            Func<ResourceContext<Company>, IEdmNavigationProperty, Uri> linkFactory = (eic, np) => eic.GenerateNavigationPropertyLink(np, false);
            companyConfiguration.HasNavigationPropertyLink(companyPartners, linkFactory, true);
            companyConfiguration.EntityType.Action("ResetDataSource");
            companyConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();

            return builder.GetEdmModel();
        }

        public static IEdmModel GetConventionModel(string singletonName)
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            EntitySetConfiguration<Partner> partnersConfiguration = builder.EntitySet<Partner>("Partners");
            EntityTypeConfiguration<Partner> partnerConfiguration = partnersConfiguration.EntityType;
            partnerConfiguration.Collection.Action("ResetDataSource");

            SingletonConfiguration<Company> companyConfiguration = builder.Singleton<Company>(singletonName);
            companyConfiguration.EntityType.Action("ResetDataSource");
            companyConfiguration.EntityType.Function("GetPartnersCount").Returns<int>();

            builder.Namespace = typeof (Company).Namespace;

            return builder.GetEdmModel();
        }
    }
}
