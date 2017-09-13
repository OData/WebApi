// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Containment
{
    public class AccountsController : ODataController
    {
        private static ContainmentDataSource _dataSource = null;
        public AccountsController()
        {
            if (_dataSource == null)
            {
                _dataSource = new ContainmentDataSource();
            }
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_dataSource.Accounts.AsQueryable());
        }

        [EnableQuery]
        public IHttpActionResult GetAccountsFromPremiumAccount()
        {
            return Ok(_dataSource.Accounts.OfType<PremiumAccount>().AsQueryable());
        }

        [EnableQuery]
        public IHttpActionResult GetPayinPIsFromAccount(int key)
        {
            var payinPIs = _dataSource.Accounts.Single(a => a.AccountID == key).PayinPIs;
            return Ok(payinPIs);
        }

        [EnableQuery]
        [ODataRoute("Accounts({key})/PayinPIs")]
        public IHttpActionResult GetPayinPIsFromAccountImport(int key)
        {
            var payinPIs = _dataSource.Accounts.Single(a => a.AccountID == key).PayinPIs;
            return Ok(payinPIs);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("Accounts({key})/PayinPIs({navKey})")]
        public IHttpActionResult GetSinglePayinPIFromAccount(int key, int navKey)
        {
            var payinPIs = _dataSource.Accounts.Single(a => a.AccountID == key).PayinPIs;
            var payinPI = payinPIs.Single(pi => pi.PaymentInstrumentID == navKey);
            return Ok(payinPI);
        }

        [EnableQuery]
        [HttpGet]
        [ODataRoute("Accounts({key})/PayinPIs({navKey})/Statement")]
        public IHttpActionResult GetStatementFromPaymentInstument(int key, int navKey)
        {
            var payinPIs = _dataSource.Accounts.Single(a => a.AccountID == key).PayinPIs;
            var payinPI = payinPIs.Single(pi => pi.PaymentInstrumentID == navKey);
            var statement = payinPI.Statement;

            IEdmEntityType productType = GetEdmEntityTypeOfStatement();
            EdmEntityObject statementObject = new EdmEntityObject(productType);
            var properties = typeof(Statement).GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                statementObject.TrySetPropertyValue(propertyInfo.Name, propertyInfo.GetValue(statement));
            }
            return Ok(statementObject);
        }

        [EnableQuery]
        [HttpPut]
        [ODataRoute("Accounts({key})/PayinPIs({navKey})/Statement")]
        public IHttpActionResult PutStatement(int key, int navKey, EdmEntityObject statementObject)
        {
            Statement newStatement = new Statement();
            var properties = typeof(Statement).GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                object value;
                statementObject.TryGetPropertyValue(propertyInfo.Name, out value);
                propertyInfo.SetValue(newStatement, value);
            }
            return Ok(newStatement);
        }

        [HttpGet]
        [HttpDelete]
        [ODataRoute("Accounts({key})/PayinPIs({navKey})/Statement")]
        public IHttpActionResult DeleteStatementFromPaymentInstument(int key, int navKey)
        {
            var payinPIs = _dataSource.Accounts.Single(a => a.AccountID == key).PayinPIs;
            var payinPI = payinPIs.Single(pi => pi.PaymentInstrumentID == navKey);
            payinPI.Statement = null;
            return StatusCode(HttpStatusCode.NoContent);
        }

        [EnableQuery]
        public IHttpActionResult GetPayoutPIFromAccount(int key)
        {
            var payoutPI = _dataSource.Accounts.Single(a => a.AccountID == key).PayoutPI;
            return Ok(payoutPI);
        }

        [EnableQuery]
        public IHttpActionResult GetGiftCardFromPremiumAccount(int key)
        {
            var giftCard = _dataSource.Accounts.OfType<PremiumAccount>().Single(a => a.AccountID == key).GiftCard;
            return Ok(giftCard);
        }

        [EnableQuery]
        public IHttpActionResult GetPayinPIsCountFromAccount(int key)
        {
            var payinPIs = _dataSource.Accounts.Single(a => a.AccountID == key).PayinPIs;
            return Ok(payinPIs.Count());
        }

        public IHttpActionResult Get(int key)
        {
            return Ok(_dataSource.Accounts.Single(a => a.AccountID == key));
        }

        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.Containment.PremiumAccount/GiftCard/$ref")]
        public IHttpActionResult GetAssociationLinkOfGiftCard(int key)
        {
            var serviceRootUri = GetServiceRootUri();
            var entityId = string.Format("{0}/Accounts({1})/WebStack.QA.Test.OData.Containment.PremiumAccount/GiftCard", serviceRootUri, key);
            return Ok(new Uri(entityId));
        }

        // GET ~/Accounts({key})/PayoutPI/$ref
        public IHttpActionResult GetRef(int key, string navigationProperty)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == key);
            var serviceRootUri = GetServiceRootUri();
            var entityId = string.Format("{0}/Accounts({1})/{2}", serviceRootUri, key, navigationProperty);
            return Ok(new Uri(entityId));
        }

        [ODataRoute("Accounts({key})/PayinPIs/$ref")]
        public IHttpActionResult GetAssociationLinkOfPayinPIs(int key)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == key);
            var serviceRootUri = GetServiceRootUri();
            IList<Uri> uris = new List<Uri>();
            account.PayinPIs.ForEach(pi => uris.Add(new Uri(string.Format("{0}/Accounts({1})/PayinPIs({2})", serviceRootUri, key, pi.PaymentInstrumentID))));
            return Ok(uris);
        }

        public IHttpActionResult Post(Account account)
        {
            account.AccountID = 300;
            if (account.PayinPIs == null)
            {
                account.PayinPIs = new List<PaymentInstrument>();
            }
            _dataSource.Accounts.Add(account);
            return Created(account);
        }

        public IHttpActionResult Put(int key, Account account)
        {
            var originalAccount = _dataSource.Accounts.Single(a => a.AccountID == key);
            account.AccountID = originalAccount.AccountID;
            if (account.PayinPIs == null)
            {
                account.PayinPIs = new List<PaymentInstrument>();
            }
            _dataSource.Accounts.Remove(originalAccount);
            _dataSource.Accounts.Add(account);
            return Ok(account);
        }

        [HttpPatch]
        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.Containment.PremiumAccount")]
        public IHttpActionResult PatchPremiumAccount(int key, Delta<PremiumAccount> delta)
        {
            var originalAccount = _dataSource.Accounts.Single(a => a.AccountID == key) as PremiumAccount;
            delta.TrySetPropertyValue("AccountID", originalAccount.AccountID); // It is the key property, and should not be updated.
            delta.Patch(originalAccount);
            return Ok(originalAccount);
        }

        [ODataRoute("Accounts({key})/WebStack.QA.Test.OData.Containment.PremiumAccount")]
        public IHttpActionResult Delete(int key)
        {
            var originalAccount = _dataSource.Accounts.Single(a => a.AccountID == key);
            _dataSource.Accounts.Remove(originalAccount);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST ~/Accounts(100)/PayinPIs
        public IHttpActionResult PostToPayinPIsFromAccount(int key, PaymentInstrument pi)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == key);
            pi.PaymentInstrumentID = account.PayinPIs.Max(p => p.PaymentInstrumentID) + 1;
            account.PayinPIs.Add(pi);
            return Created(pi);
        }

        // PUT ~/Accounts(100)/PayoutPI
        [ODataRoute("Accounts({accountId})/PayoutPI")]
        public IHttpActionResult PutToPayoutPIFromAccount(int accountId, [FromBody]PaymentInstrument paymentInstrument)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            account.PayoutPI = paymentInstrument;
            return Ok(paymentInstrument);
        }

        // PUT ~/Accounts(100)/PayinPIs(101)
        [ODataRoute("Accounts({accountId})/PayinPIs({paymentInstrumentId})")]
        public IHttpActionResult PutToPayinPI(int accountId, int paymentInstrumentId, [FromBody]PaymentInstrument paymentInstrument)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var originalPi = account.PayinPIs.Single(p => p.PaymentInstrumentID == paymentInstrumentId);
            originalPi.FriendlyName = paymentInstrument.FriendlyName;
            originalPi.Statement = paymentInstrument.Statement;
            return Ok(paymentInstrument);
        }

        // PATCH ~/Accounts(100)/PayinPIs(101)
        [ODataRoute("Accounts({accountId})/PayinPIs({paymentInstrumentId})")]
        public IHttpActionResult PatchToPayinPI(int accountId, int paymentInstrumentId, [FromBody]Delta<PaymentInstrument> delta)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var originalPi = account.PayinPIs.Single(p => p.PaymentInstrumentID == paymentInstrumentId);
            delta.Patch(originalPi);
            return Ok(originalPi);
        }

        // PATCH ~/Accounts(200)/WebStack.QA.Test.OData.Containment.PremiumAccount/GiftCard
        [HttpPatch]
        [ODataRoute("Accounts({accountId})/WebStack.QA.Test.OData.Containment.PremiumAccount/GiftCard")]
        public IHttpActionResult PatchToGiftCardFromPremiumAccount(int accountId, [FromBody] Delta<GiftCard> giftCard)
        {
            var account = _dataSource.Accounts.OfType<PremiumAccount>().Single(a => a.AccountID == accountId);
            var originalGiftCard = account.GiftCard;
            giftCard.Patch(originalGiftCard);
            return Ok(originalGiftCard);
        }

        // DELETE ~/Accounts(100)/PayinPIs(101)
        [HttpDelete]
        [ODataRoute("Accounts({accountId})/PayinPIs({paymentInstrumentId})")]
        public IHttpActionResult DeletePayinPIFromAccount(int accountId, int paymentInstrumentId)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var originalPi = account.PayinPIs.Single(p => p.PaymentInstrumentID == paymentInstrumentId);
            if (account.PayinPIs.Remove(originalPi))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }


        // DELETE ~/Accounts(100)/PayinPIs/$ref
        [HttpDelete]
        public IHttpActionResult DeleteRef(int key, int relatedKey, string navigationProperty)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == key);

            if (navigationProperty != "PayinPIs")
            {
                return BadRequest();
            }

            var originalPi = account.PayinPIs.Single(p => p.PaymentInstrumentID == relatedKey);
            if (account.PayinPIs.Remove(originalPi))
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            else
            {
                return StatusCode(HttpStatusCode.InternalServerError);
            }
        }

        // Delete ~/Accounts(100)/PayoutPI
        [HttpDelete]
        [ODataRoute("Accounts({accountId})/PayoutPI")]
        public IHttpActionResult DeletePayoutPIFromAccount(int accountId)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            account.PayoutPI = null;
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST ~/Accounts(100)/PayinPIs/Namespace.Clear
        [HttpPost]
        [ODataRoute("Accounts({accountId})/PayinPIs/WebStack.QA.Test.OData.Containment.Clear")]
        public IHttpActionResult ClearPayoutPIFromAccount(int accountId, ODataActionParameters parameters)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var nameContains = parameters["nameContains"] as string;
            var candidatePayinPIs = account.PayinPIs.Where(pi => pi.FriendlyName.Contains(nameContains)).ToList();
            foreach (var candidate in candidatePayinPIs)
            {
                account.PayinPIs.Remove(candidate);
            }
            return Ok(candidatePayinPIs.Count());
        }

        // POST ~/Accounts(100)/PayinPIs(101)/Namespace.Delete
        [HttpPost]
        [ODataRoute("Accounts({accountId})/PayinPIs({paymentInstrumentId})/WebStack.QA.Test.OData.Containment.Delete")]
        public IHttpActionResult DeleteAGivenPayinPIFromAccount(int accountId, int paymentInstrumentId)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var paymentInstrument = account.PayinPIs.Single(pi => pi.PaymentInstrumentID == paymentInstrumentId);
            account.PayinPIs.Remove(paymentInstrument);
            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST ~/Accounts(100)/PayoutPI/Namespace.Delete
        [HttpPost]
        [ODataRoute("Accounts({accountId})/PayoutPI/WebStack.QA.Test.OData.Containment.Delete")]
        public IHttpActionResult SetPayoutPiToNull(int accountId)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            account.PayoutPI = null;
            return StatusCode(HttpStatusCode.NoContent);
        }

        // Bug 2021-Should support Action/Function returns contained entities.
        // POST ~/Accounts(100)/PayoutPI/Namespace.Duplicate
        [HttpPost]
        //[ODataRoute("Accounts({accountId})/PayinPIs({piId})/WebStack.QA.Test.OData.Containment.Duplicate")]
        public IHttpActionResult DuplicatePayinPI(int accountId, int piId)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var paymentInstrument = account.PayinPIs.Single(pi => pi.PaymentInstrumentID == piId);
            Statement newStatement = null;
            if (paymentInstrument.Statement != null)
            {
                newStatement = new Statement()
                {
                    StatementID = 1,
                    TransactionDescription = (string)paymentInstrument.Statement.TransactionDescription.Clone(),
                    Amount = paymentInstrument.Statement.Amount,
                };
            }
            var newPI = new PaymentInstrument()
            {
                PaymentInstrumentID = account.PayinPIs.Max(pi => pi.PaymentInstrumentID) + 1,
                FriendlyName = paymentInstrument.FriendlyName + " - Copy",
                Statement = newStatement,
            };
            account.PayinPIs.Add(newPI);

            return Ok(newPI);
        }

        // GET ~/Accounts(100)/PayinPIs/Namespace.GetCount)
        [HttpGet]
        [ODataRoute("Accounts({accountId})/PayinPIs/WebStack.QA.Test.OData.Containment.GetCount(nameContains={name})")]
        public IHttpActionResult GetPayinPIsCountWhoseNameContainsGivenValue(int accountId, [FromODataUri]string name)
        {
            var account = _dataSource.Accounts.Single(a => a.AccountID == accountId);
            var count = account.PayinPIs.Where(pi => pi.FriendlyName.Contains(name)).Count();

            return Ok(count);
        }

        [ODataRoute("ResetDataSource")]
        public IHttpActionResult ResetDataSource()
        {
            _dataSource = new ContainmentDataSource();
            return StatusCode(HttpStatusCode.NoContent);
        }

        private string GetServiceRootUri()
        {
            // TODO: open a bug about adding ServiceRootUri to ODataProperites().
            var routeName = Request.ODataProperties().RouteName;
            ODataRoute odataRoute = Configuration.Routes[routeName] as ODataRoute;
            var prefixName = odataRoute.RoutePrefix;
            var requestUri = Request.RequestUri.ToString();
            var serviceRootUri = requestUri.Substring(0, requestUri.IndexOf(prefixName) + prefixName.Length);
            return serviceRootUri;
        }

        private IEdmEntityType GetEdmEntityTypeOfStatement()
        {
            IEdmModel edmModel = Request.GetModel();
            IEdmEntityType statementType = (IEdmEntityType)edmModel.FindDeclaredType(typeof(Statement).FullName);
            return statementType;
        }
    }

    public class AnonymousAccountController : ODataController
    {
        private static ContainmentDataSource _dataSource = null;
        public AnonymousAccountController()
        {
            if (_dataSource == null)
            {
                _dataSource = new ContainmentDataSource();
            }
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_dataSource.AnonymousAccount);
        }

        [EnableQuery]
        public IHttpActionResult GetPayinPIs()
        {
            var payinPIs = _dataSource.AnonymousAccount.PayinPIs;
            return Ok(payinPIs);
        }

        [EnableQuery]
        public IHttpActionResult GetPayoutPI()
        {
            var payoutPI = _dataSource.AnonymousAccount.PayoutPI;
            return Ok(payoutPI);
        }

    }
}
