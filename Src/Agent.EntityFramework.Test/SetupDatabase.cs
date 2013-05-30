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
            var patch_only = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Log.EndSession();
        }
    }
}
