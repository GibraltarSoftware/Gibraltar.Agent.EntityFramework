using Gibraltar.Agent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Agent.EntityFramework.Test
{
    [TestClass]
    public class SetupDatabase
    {
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            Log.StartSession();
            var patch_only = System.Data.Entity.SqlServer.SqlProviderServices.Instance;  //this is just here to force this particular provider into RAM since it's new to EF6
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Log.EndSession();
        }
    }
}
