#region File Header and License
// /*
//    UpdateDataInModel.cs
//    Copyright 2013 Gibraltar Software, Inc.
//    
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// */
#endregion
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
