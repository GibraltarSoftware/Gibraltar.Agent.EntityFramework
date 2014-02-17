#region File Header and License
// /*
//    SetupDatabase.cs
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
            Log.Initializing += (sender, args) =>
                                    {
                                        var publisherConfig = args.Configuration.Publisher;
                                        publisherConfig.ProductName = "Loupe";
                                        publisherConfig.ApplicationName = "Entity Framework Agent Tests";
                                        publisherConfig.ApplicationType = ApplicationType.Console;
                                    };
            Log.StartSession();
            var patch_only = System.Data.Entity.SqlServer.SqlProviderServices.Instance;  //this is just here to force this particular provider into RAM since it's new to EF6
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Log.EndSession();
        }

        private void SeedDatabase()
        {
            
        }
    }
}
