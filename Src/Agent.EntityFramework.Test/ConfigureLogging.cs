#region File Header and License
// /*
//    ConfigureLogging.cs
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
