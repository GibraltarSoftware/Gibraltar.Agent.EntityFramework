Loupe Agent for Entity Framework 6.0
===================

This agent adds Entity Framework-specific monitoring features.  If you don't need
to modify the source code just download the latest [Loupe Agent for Entity Framework](https://nuget.org/packages/Gibraltar.Agent.EntityFramework/).  
It extends the [Loupe Agent](https://nuget.org/packages/Gibraltar.Agent/) so you can 
use any viewer for Loupe to visualize network information

Using the Agent
---------------

To activate the agent it isn't enough to simply deploy it with your project, you need to make 
one call to register it with Entity Framework.  The call can be made multiple times safely
(and without causing a dobule regsitration).  Once registered with Entity Framework it will
automatically record information for every EF 6.0 context in the application domain.

```C#
//Register the Interceptor
Gibraltar.Agent.EntityFramework.LoupeCommandInterceptor.Register();
```


Implementation Notes
--------------------

This extension works only with Entity Framework 6.0 RC 1 Build 20726 and later because it relies on the
new database command interception features built into EF 6.0. Until EF 6 RC is published
you will need to pull the nightly builds for Entity Framework from here:
http://entityframework.codeplex.com/wikipage?title=Nightly%20Builds

It is compiled for .NET 4.0 but is compatible with both .NET 4.0 and .NET 4.5.


Building the Agent
------------------

This project is designed for use with Visual Studio 2012 with NuGet package restore enabled.
When you build it the first time it will retrieve dependencies from NuGet.

Contributing
------------

Feel free to branch this project and contribute a pull request to the development branch. 
If your changes are incorporated into the master version they'll be published out to NuGet for
everyone to use!