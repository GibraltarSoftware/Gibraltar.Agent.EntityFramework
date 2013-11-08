using System;
using System.Configuration;

namespace Gibraltar.Agent.EntityFramework.Configuration
{
    /// <summary>
    /// Configuration options for the Loupe Agent for Entity Framework
    /// </summary>
    public class EntityFrameworkElement : ConfigurationSection
    {
        /// <summary>
        /// The root log category for this agent
        /// </summary>
        internal const string LogCategory = "Data Access";

        /// <summary>
        /// Determines if any agent functionality should be enabled.  Defaults to true.
        /// </summary>
        /// <remarks>To disable the entire agent set this option to false.  Even if individual
        /// options are enabled they will be ignored if this is set to false.</remarks>
        [ConfigurationProperty("enabled", DefaultValue = true, IsRequired = false)]
        public bool Enabled { get { return (bool)this["enabled"]; } set { this["enabled"] = value; } }

        /// <summary>
        /// Determines if the call stack for each operation should be recorded
        /// </summary>
        /// <remarks>This is useful for determining what application code causes each query</remarks>
        [ConfigurationProperty("logCallStack", DefaultValue = false, IsRequired = false)]
        public bool LogCallStack { get { return (bool)this["logCallStack"]; } set { this["logCallStack"] = value; } }

        /// <summary>
        /// The severity used for log messages for the Entity Framework trace message. Defaults to Verbose.
        /// </summary>
        [ConfigurationProperty("queryMessageSeverity", DefaultValue = LogMessageSeverity.Verbose, IsRequired = false)]
        public LogMessageSeverity QueryMessageSeverity { get { return (LogMessageSeverity)this["queryMessageSeverity"]; } set { this["queryMessageSeverity"] = value; } }

        /// <summary>
        /// Determines if a log message is written for exceptions during entity framework operations. Defaults to true.
        /// </summary>
        [ConfigurationProperty("logExceptions", DefaultValue = true, IsRequired = false)]
        public bool LogExceptions { get { return (bool)this["logExceptions"]; } set { this["logExceptions"] = value; } }

        /// <summary>
        /// The severity used for log messages for entity framework operations that throw an exception. Defaults to Error.
        /// </summary>
        [ConfigurationProperty("exceptionSeverity", DefaultValue = LogMessageSeverity.Error, IsRequired = false)]
        public LogMessageSeverity ExceptionSeverity { get { return (LogMessageSeverity)this["exceptionSeverity"]; } set { this["exceptionSeverity"] = value; } }

        /// <summary>
        /// Load the elemnt from the system configuration file, falling back to defaults if it can't be parsed
        /// </summary>
        /// <returns>A new element object</returns>
        internal static EntityFrameworkElement SafeLoad()
        {
            EntityFrameworkElement configuration = null;
            try
            {
                //see if we can get a configuration section
                configuration = ConfigurationManager.GetSection("gibraltar/entityFramework") as EntityFrameworkElement;
            }
            catch (Exception ex)
            {
                Log.Error(ex, LogCategory + ".Agent", "Unable to load the MVC Agent configuration from the config file",
                          "The default configuration will be used which will no doubtedly create unexpected behavior.  Exception:\r\n{0}", ex.Message);
            }

            return configuration ?? new EntityFrameworkElement();
        }
    }
}
