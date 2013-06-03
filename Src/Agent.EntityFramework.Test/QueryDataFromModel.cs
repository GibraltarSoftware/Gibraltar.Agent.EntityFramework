using System;
using System.Linq;
using Agent.EntityFramework.Test.Entities;
using Gibraltar.Agent;
using Gibraltar.Agent.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agent.EntityFramework.Test
{
    [TestClass]
    public class QueryDataFromModel
    {
        private const string LogCategory = "Unit Tests.Query Data";

        [TestInitialize]
        public void RegisterInterceptor()
        {
            LoupeCommandInterceptor.Register();
        }

        [TestMethod]
        public void SimpleQuery()
        {
            using (var ctx = new NorthwindEntities())
            {
                var results = from c in ctx.Customers select c;

                results.Count();
            }
        }

    }
}
