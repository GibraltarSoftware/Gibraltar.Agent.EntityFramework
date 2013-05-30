using Gibraltar.Agent.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agent.EntityFramework.Test
{
    [TestClass]
    public class ConfigureLogging
    {
        [TestMethod]
        public void RegisterInterceptor()
        {
            LoupeCommandInterceptor.Register();
        }

        [TestMethod]
        public void DuplicateRegistration()
        {
            LoupeCommandInterceptor.Register(); //this should be a duplicate.
            LoupeCommandInterceptor.Register(); //but now we can be sure
        }
    }
}
