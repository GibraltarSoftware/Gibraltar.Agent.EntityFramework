using System;
using System.Linq;
using Agent.EntityFramework.Test.Entities;
using Gibraltar.Agent;
using Gibraltar.Agent.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agent.EntityFramework.Test
{
    [TestClass]
    public class UpdateDataInModel
    {
        private const string LogCategory = "Unit Tests.Update Data";

        [TestInitialize]
        public void RegisterInterceptor()
        {
            LoupeCommandInterceptor.Register();
        }

        [TestMethod]
        public void SimpleUpdate()
        {
            try
            {
                using (var ctx = new NorthwindEntities())
                {
                    var newCustomer = ctx.Customers.Add(new Customer());
                    newCustomer.Address = "Address Line 1";
                    newCustomer.City = "Springfield";
                    newCustomer.CustomerID = "AE" + ctx.Customers.Count();
                    newCustomer.CompanyName = "Our Company" + ctx.Customers.Count();
                    newCustomer.ContactName = "John Doe";
                    newCustomer.ContactTitle = "Senior Manager";
                    newCustomer.Region = "midwest";
                    newCustomer.PostalCode = "50501";

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.RecordException(ex, LogCategory, true);
                throw; //we want to be sure we fail the unit test.
            }
        }

        [TestMethod]
        public void TransactionalUpdate()
        {
            try
            {
                using (var ctx = new NorthwindEntities())
                {
                    var newCustomer = ctx.Customers.Add(new Customer());
                    newCustomer.Address = "Address Line 1";
                    newCustomer.City = "Springfield";
                    newCustomer.CustomerID = "AE" + ctx.Customers.Count();
                    newCustomer.CompanyName = "Our Company" + ctx.Customers.Count();
                    newCustomer.ContactName = "John Doe";
                    newCustomer.ContactTitle = "Senior Manager";
                    newCustomer.Region = "midwest";
                    newCustomer.PostalCode = "50501";

                    ctx.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.RecordException(ex, LogCategory, true);
                throw; //we want to be sure we fail the unit test.
            }
        }
    }
}
