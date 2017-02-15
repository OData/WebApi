using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Default;
using ODataSample.Web.Models;

namespace ODataSample.Consumer
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Run(AsyncFunc);
            SyncFunc();
        }

        public static void SyncFunc()
        {
            var service = new Default.Container(new Uri("http://localhost:5000/odata"));

            var acrionResult = service.Customers.Ping(1).GetValue();
            Contract.Assert(acrionResult);

            var acrionResult2 = service.Customers.Ping(123123).GetValue();
            Contract.Assert(!acrionResult2);

            var functionResult = service.Customers.Pong(1).GetValue();
            Contract.Assert(functionResult);

            var functionResult2 = service.Customers.Pong(1123123).GetValue();
            Contract.Assert(!functionResult2);

            var customers = service.Customers;
            var customer1 = service.Customers.ByKey(1).GetValue();
            Contract.Assert(customers.First().FirstName == customer1.FirstName);
            Contract.Assert(customers.First().LastName == customer1.LastName);

            //var orderedCustomers = service.Customers.OrderBy(x => x.FirstName);
            // it's broken 

            service.AddToCustomers(new Customer() { FirstName = "Mietek", LastName = "Trzepak", CustomerId = 88888 });
            service.SaveChanges();
            var customerMietek = service.Customers.ByKey(88888).GetValue();
            Contract.Assert(customerMietek.FirstName == "Mietek");

        }

        public static async Task AsyncFunc()
        {
            var service = new Default.Container(new Uri("http://localhost:5000/odata"));

            var acrionResult = service.Customers.Ping(1).GetValueAsync();
            Contract.Assert(await acrionResult);

            var acrionResult2 = await service.Customers.Ping(123123).GetValueAsync();
            Contract.Assert(!acrionResult2);

            var functionResult = service.Customers.Pong(1).GetValueAsync();
            Contract.Assert(await functionResult);

            var functionResult2 = await service.Customers.Pong(1123123).GetValueAsync();
            Contract.Assert(!functionResult2);

            var customers = service.Customers;
            var customer1 = await service.Customers.ByKey(1).GetValueAsync();
            Contract.Assert(customers.First().FirstName == customer1.FirstName);
            Contract.Assert(customers.First().LastName == customer1.LastName);

            // var orderedCustomers = service.Customers.OrderBy(x => x.FirstName);
            // it's broken 

            service.AddToCustomers(new Customer() { FirstName = "Mietek", LastName = "Trzepak", CustomerId = 99999 });
            await service.SaveChangesAsync();
            var customerMietek = await service.Customers.ByKey(99999).GetValueAsync();
            Contract.Assert(customerMietek.FirstName == "Mietek");
        }
    }
}
