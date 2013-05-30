Loupe Agent for ASP.NET WebForms
===================

This agent adds ASP.NET WebForms-specific monitoring features.  If you don't need
to modify the source code just download the latest [Loupe Agent for ASP.NET](https://nuget.org/packages/Gibraltar.Agent.Web/).  
It extends the [Loupe Agent](https://nuget.org/packages/Gibraltar.Agent/) so you can 
use any viewer for Loupe to visualize network information.


Implementation Notes
--------------------

Since Loupe supports .NET 2.0 and later and the WebForms capabilties are available
in .NET 2.0 this agent targets .NET 2.0 as well.  Due to the built-in compatibility handling in the
.NET runtime it can be used by any subsequent verison of .NET so there's no need for a .NET 4.0 or later
version unless modifying to support something only available in .NET 4.0 or later.


Building the Agent
------------------

This project is designed for use with Visual Studio 2012 with NuGet package restore enabled.
When you build it the first time it will retrieve dependencies from NuGet.

Contributing
------------

Feel free to branch this project and contribute a pull request to the development branch. 
If your changes are incorporated into the master version they'll be published out to NuGet for
everyone to use!