// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Core.UriParser.Semantic;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.TestCommon;

namespace System.Web.OData.Builder
{
    public class ContainmentPathBuilderTest
    {
        private IEdmModel _model;
        private Uri _serviceRoot = new Uri("http://host/service/", UriKind.Absolute);

        [Theory]
        // Contained EntitySet direct access
        [InlineData("Accounts(101)/MyPaymentInstruments", "Accounts(101)")]
        // Contained EntitySet in another contained EntitySet
        [InlineData(
            "Accounts(101)/MyPaymentInstruments(101904)/ContainedBillingAddresses",
            "Accounts(101)/MyPaymentInstruments(101904)")]
        // With redundant type cast
        [InlineData("People(1)/ns.VIP/MyAddresses", "People(1)")]
        [InlineData("People/ns.VIP(1)/MyAddresses", "People(1)")]
        // With consecutive redundant type cast
        [InlineData("People(1)/ns.VIP/ns.VIP/MyAddresses", "People(1)")]
        [InlineData("People/ns.VIP(1)/ns.VIP/MyAddresses", "People(1)")]
        [InlineData("People/ns.VIP/ns.VIP(1)/MyAddresses", "People(1)")]
        // With multiple redundant type casts
        [InlineData("Clubs(1)/ns.SeniorClub/Members(1)/ns.VIP/MyAddresses", "Clubs(1)/Members(1)")]
        [InlineData("Clubs/ns.SeniorClub(1)/Members(1)/ns.VIP/MyAddresses", "Clubs(1)/Members(1)")]
        [InlineData("Clubs/ns.SeniorClub(1)/Members/ns.VIP(1)/MyAddresses", "Clubs(1)/Members(1)")]
        // With type cast at the end
        [InlineData("Accounts(101)/MyPaymentInstruments/ns.PaymentInstrument", "Accounts(101)")]
        // With useful type cast
        [InlineData("People(1)/ns.VIP/MyBenefits", "People(1)/ns.VIP")]
        [InlineData("People/ns.VIP(1)/MyBenefits", "People(1)/ns.VIP")]
        // Ensure cast to the owning type replace unecessarily-zealous cast
        [InlineData("People(1)/ns.VIP/Benefits", "People(1)/ns.SpecialPerson")]
        [InlineData("People/ns.VIP(1)/Benefits", "People(1)/ns.SpecialPerson")]
        // With multiple mixed type casts
        [InlineData("Clubs(1)/ns.SeniorClub/Members(1)/ns.VIP/MyBenefits", "Clubs(1)/Members(1)/ns.VIP")]
        [InlineData("Clubs/ns.SeniorClub(1)/Members(1)/ns.VIP/MyBenefits", "Clubs(1)/Members(1)/ns.VIP")]
        [InlineData("Clubs/ns.SeniorClub(1)/Members/ns.VIP(1)/MyBenefits", "Clubs(1)/Members(1)/ns.VIP")]
        // With redundant EntitySet path
        [InlineData("Clubs(1)/ns.SeniorClub/Members(1)/ns.VIP/MyAccounts(101)/MyPaymentInstruments", "Accounts(101)")]
        [InlineData("Clubs/ns.SeniorClub(1)/Members(1)/ns.VIP/MyAccounts(101)/MyPaymentInstruments", "Accounts(101)")]
        [InlineData("Clubs/ns.SeniorClub(1)/Members/ns.VIP(1)/MyAccounts(101)/MyPaymentInstruments", "Accounts(101)")]
        // With single valued navigation property (point to entity set)
        [InlineData("People(1)/MyLatestAccount/MyPaymentInstruments", "People(1)/MyLatestAccount")]
        // With single valued navigation property (point to singleton)
        [InlineData("People(1)/MyPermanentAccount/MyPaymentInstruments", "PermanentAccount")]
        public void TryComputeCanonicalContainingPath_ForContainedEntitySetDirectAccess(
            string resourcePath,
            string expectedContainingPath)
        {
            // Arrange
            var path = CreatePathFromUri(new Uri(resourcePath, UriKind.Relative));
            var builder = new ContainmentPathBuilder();

            // Act
            path = builder.TryComputeCanonicalContainingPath(path);

            // Assert
            var uri = CreateUriFromPath(path);
            uri = _serviceRoot.MakeRelativeUri(uri);
            Assert.Equal(expectedContainingPath, uri.ToString());
        }

        private IEdmModel GetModel()
        {
            if (_model != null)
            {
                return _model;
            }

            var model = new EdmModel();

            // EntityContainer: Service
            var container = new EdmEntityContainer("ns", "Service");
            model.AddElement(container);

            // EntityType: Address
            var address = new EdmEntityType("ns", "Address");
            var addressId = address.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            address.AddKeys(addressId);
            model.AddElement(address);

            // EntitySet: Addresses
            var addresses = container.AddEntitySet("Addresses", address);

            // EntityType: PaymentInstrument
            var paymentInstrument = new EdmEntityType("ns", "PaymentInstrument");
            var paymentInstrumentId = paymentInstrument.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            paymentInstrument.AddKeys(paymentInstrumentId);
            var billingAddresses = paymentInstrument.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "BillingAddresses",
                Target = address,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            paymentInstrument.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "ContainedBillingAddresses",
                Target = address,
                TargetMultiplicity = EdmMultiplicity.Many,
                ContainsTarget = true
            });
            model.AddElement(paymentInstrument);

            // EntityType: Account
            var account = new EdmEntityType("ns", "Account");
            var accountId = account.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            account.AddKeys(accountId);
            var myPaymentInstruments = account.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "MyPaymentInstruments",
                Target = paymentInstrument,
                TargetMultiplicity = EdmMultiplicity.Many,
                ContainsTarget = true
            });
            model.AddElement(account);

            // EntitySet: Accounts
            var accounts = container.AddEntitySet("Accounts", account);

            var paymentInstruments = accounts.FindNavigationTarget(myPaymentInstruments) as EdmNavigationSource;
            Assert.NotNull(paymentInstruments);
            paymentInstruments.AddNavigationTarget(billingAddresses, addresses);

            // EntityType: Person
            var person = new EdmEntityType("ns", "Person");
            var personId = person.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            person.AddKeys(personId);
            var myAccounts = person.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "MyAccounts",
                Target = account,
                TargetMultiplicity = EdmMultiplicity.Many
            });
            var myPermanentAccount = person.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "MyPermanentAccount",
                Target = account,
                TargetMultiplicity = EdmMultiplicity.One
            });
            var myLatestAccount = person.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "MyLatestAccount",
                Target = account,
                TargetMultiplicity = EdmMultiplicity.One
            });
            person.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "MyAddresses",
                Target = address,
                TargetMultiplicity = EdmMultiplicity.Many,
                ContainsTarget = true
            });
            model.AddElement(person);

            // EntityType: Benefit
            var benefit = new EdmEntityType("ns", "Benefit");
            var benefitId = benefit.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            benefit.AddKeys(benefitId);
            model.AddElement(benefit);

            // EntityType: SpecialPerson
            var specialPerson = new EdmEntityType("ns", "SpecialPerson", person);
            specialPerson.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Benefits",
                Target = benefit,
                TargetMultiplicity = EdmMultiplicity.Many,
                ContainsTarget = true
            });
            model.AddElement(specialPerson);

            // EntityType: VIP
            var vip = new EdmEntityType("ns", "VIP", specialPerson);
            vip.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "MyBenefits",
                Target = benefit,
                TargetMultiplicity = EdmMultiplicity.Many,
                ContainsTarget = true
            });
            model.AddElement(vip);

            // EntitySet: People
            var people = container.AddEntitySet("People", person);
            people.AddNavigationTarget(myAccounts, accounts);
            people.AddNavigationTarget(myLatestAccount, accounts);

            // EntityType: Club
            var club = new EdmEntityType("ns", "Club");
            var clubId = club.AddStructuralProperty("Id", EdmPrimitiveTypeKind.Int32);
            club.AddKeys(clubId);
            var members = club.AddUnidirectionalNavigation(new EdmNavigationPropertyInfo
            {
                Name = "Members",
                Target = person,
                TargetMultiplicity = EdmMultiplicity.Many,
                ContainsTarget = true
            });

            // EntityType: SeniorClub
            var seniorClub = new EdmEntityType("ns", "SeniorClub", club);
            model.AddElement(seniorClub);

            // EntitySet: Clubs
            var clubs = container.AddEntitySet("Clubs", club);
            var membersInClub = clubs.FindNavigationTarget(members) as EdmNavigationSource;
            membersInClub.AddNavigationTarget(myAccounts, accounts);

            // Singleton: PermanentAccount
            var permanentAccount = container.AddSingleton("PermanentAccount", account);
            people.AddNavigationTarget(myPermanentAccount, permanentAccount);

            _model = model;

            return _model;
        }

        private ODataPath CreatePathFromUri(Uri requestUri)
        {
            var parser = new ODataUriParser(GetModel(), _serviceRoot, requestUri);
            return parser.ParsePath();
        }

        private Uri CreateUriFromPath(ODataPath path)
        {
            var segments = path;
            var computedUri = _serviceRoot;

            // Append each segment to base uri
            foreach (ODataPathSegment segment in segments)
            {
                KeySegment keySegment = segment as KeySegment;
                if (keySegment != null)
                {
                    computedUri = AppendKeyExpression(computedUri, keySegment.Keys);
                    continue;
                }

                EntitySetSegment entitySetSegment = segment as EntitySetSegment;
                if (entitySetSegment != null)
                {
                    computedUri = AppendSegment(computedUri, entitySetSegment.EntitySet.Name);
                    continue;
                }

                SingletonSegment singletonSegment = segment as SingletonSegment;
                if (singletonSegment != null)
                {
                    computedUri = AppendSegment(computedUri, singletonSegment.Singleton.Name);
                    continue;
                }

                var typeSegment = segment as TypeSegment;
                if (typeSegment != null)
                {
                    var edmType = typeSegment.EdmType;
                    if (edmType.TypeKind == EdmTypeKind.Collection)
                    {
                        var collectionType = (IEdmCollectionType)edmType;
                        edmType = collectionType.ElementType.Definition;
                    }

                    computedUri = AppendSegment(computedUri, edmType.FullTypeName());
                }
                else
                {
                    computedUri = AppendSegment(
                        computedUri,
                        ((NavigationPropertySegment)segment).NavigationProperty.Name);
                }
            }

            return computedUri;
        }

        private static Uri AppendKeyExpression(Uri baseUri, IEnumerable<KeyValuePair<string, object>> keyProperties)
        {
            StringBuilder builder = new StringBuilder(baseUri.AbsoluteUri);
            List<KeyValuePair<string, object>> keys = keyProperties.ToList();

            builder.Append('(');

            bool first = true;
            foreach (KeyValuePair<string, object> property in keys)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(',');
                }

                if (keys.Count != 1)
                {
                    builder.Append(property.Key);
                    builder.Append('=');
                }

                builder.Append(property.Value);
            }

            builder.Append(')');

            return new Uri(builder.ToString(), UriKind.Absolute);
        }

        private static Uri AppendSegment(Uri baseUri, string segment)
        {
            string baseUriString = baseUri.AbsoluteUri;

            segment = Uri.EscapeDataString(segment);

            if (baseUriString[baseUriString.Length - 1] != '/')
            {
                return new Uri(baseUriString + '/' + segment, UriKind.Absolute);
            }

            return new Uri(baseUri, segment);
        }
    }
}
