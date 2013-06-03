using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gibraltar.Agent.EntityFramework.Internal;

namespace Gibraltar.Agent.EntityFramework
{
    /// <summary>
    /// Records performance and diagnostic information for Entity Framework
    /// </summary>
    public class LoupeCommandInterceptor : IDbCommandInterceptor
    {
        private const string LogSystem = "Gibraltar";
        private const string LogCategory = "Data Access.Query";

        private static readonly object s_Lock = new object();
        private static bool s_IsRegistered = false; //PROTECTED BY LOCK
        
        private readonly Dictionary<int, DatabaseMetric> _databaseMetrics = new Dictionary<int, DatabaseMetric>(); 

        private LoupeCommandInterceptor()
        {
            IncludeCallStack = true;
            LogExceptions = true;
        }

        /// <summary>
        /// Register the Loupe Command Interceptor with Entity Framework (safe to call multiple times)
        /// </summary>
        public static void Register()
        {
            lock(s_Lock)
            {
                if (s_IsRegistered)
                    return;

                s_IsRegistered = true;
                Interception.AddInterceptor(new LoupeCommandInterceptor());
            }
        }

        /// <summary>
        /// Indicates if the call stack to the operation should be included in the log message
        /// </summary>
        public bool IncludeCallStack { get; set; }

        /// <summary>
        /// Indicates if execution exceptions should be logged
        /// </summary>
        public bool LogExceptions { get; set; }

        /// <summary>
        /// This method is called before a call to <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery"/> or
        ///                 one of its async counterparts is made.
        /// </summary>
        /// <param name="command">The command being executed.</param><param name="interceptionContext">Contextual information associated with the call.</param>
        public void NonQueryExecuting(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            StartTrackingCommand(command, interceptionContext);
        }

        /// <summary>
        /// This method is called after a call to <see cref="M:System.Data.Common.DbCommand.ExecuteNonQuery"/>  or
        ///                 one of its async counterparts is made. This method should return the given result.
        ///                 However, the result used by Entity Framework can be changed by returning a different value.
        /// </summary>
        /// <remarks>
        /// For async operations this method is not called until after the async task has completed
        ///                 or failed.
        /// </remarks>
        /// <param name="command">The command being executed.</param><param name="interceptionContext">Contextual information associated with the call.</param>
        public void NonQueryExecuted(DbCommand command, DbCommandInterceptionContext<int> interceptionContext)
        {
            StopTrackingCommand(command, interceptionContext, interceptionContext.Result);
        }

        /// <summary>
        /// This method is called before a call to <see cref="M:System.Data.Common.DbCommand.ExecuteReader(System.Data.CommandBehavior)"/>  or
        ///                 one of its async counterparts is made.
        /// </summary>
        /// <param name="command">The command being executed.</param><param name="interceptionContext">Contextual information associated with the call.</param>
        public void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            StartTrackingCommand(command, interceptionContext);
        }

        /// <summary>
        /// This method is called after a call to <see cref="M:System.Data.Common.DbCommand.ExecuteReader(System.Data.CommandBehavior)"/>  or
        ///                 one of its async counterparts is made. This method should return the given result. However, the
        ///                 result used by Entity Framework can be changed by returning a different value.
        /// </summary>
        /// <remarks>
        /// For async operations this method is not called until after the async task has completed
        ///                 or failed.
        /// </remarks>
        /// <param name="command">The command being executed.</param><param name="interceptionContext">Contextual information associated with the call.</param>
        public void ReaderExecuted(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            StopTrackingCommand(command, interceptionContext, null);
        }

        /// <summary>
        /// This method is called before a call to <see cref="M:System.Data.Common.DbCommand.ExecuteScalar"/>  or
        ///                 one of its async counterparts is made.
        /// </summary>
        /// <param name="command">The command being executed.</param><param name="interceptionContext">Contextual information associated with the call.</param>
        public void ScalarExecuting(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            StartTrackingCommand(command, interceptionContext);
        }

        /// <summary>
        /// This method is called after a call to <see cref="M:System.Data.Common.DbCommand.ExecuteScalar"/>  or
        ///                 one of its async counterparts is made. This method should return the given result.
        ///                 However, the result used by Entity Framework can be changed by returning a different value.
        /// </summary>
        /// <remarks>
        /// For async operations this method is not called until after the async task has completed
        ///                 or failed.
        /// </remarks>
        /// <param name="command">The command being executed.</param><param name="interceptionContext">Contextual information associated with the call.</param>
        public void ScalarExecuted(DbCommand command, DbCommandInterceptionContext<object> interceptionContext)
        {
            StopTrackingCommand(command, interceptionContext, null);
        }


        private void StartTrackingContext(DbCommandInterceptionContext context)
        {
            foreach (var dbContext in context.DbContexts)
            {
                StringBuilder messageBuilder = new StringBuilder();

                var addedEntities = dbContext.ChangeTracker.Entries().Where(changeEntry => changeEntry.State == EntityState.Added).ToList();
                if (addedEntities.Count > 0)
                {
                    messageBuilder.AppendLine("Added Entities:");

                    foreach (var dbEntityEntry in addedEntities)
                    {
                        messageBuilder.AppendFormat("     {0}\r\n", dbEntityEntry.Entity.GetType());
                    }

                    messageBuilder.AppendLine();
                }

                var changedEntities = dbContext.ChangeTracker.Entries().Where(changeEntry => changeEntry.State == EntityState.Deleted).ToList();
                if (changedEntities.Count > 0)
                {
                    messageBuilder.AppendLine("Changed Entities:");

                    foreach (var dbEntityEntry in changedEntities)
                    {
                        messageBuilder.AppendFormat("     {0}\r\n", dbEntityEntry.Entity.GetType());
                    }

                    messageBuilder.AppendLine();
                }

                var removedEntities = dbContext.ChangeTracker.Entries().Where(changeEntry => changeEntry.State == EntityState.Modified).ToList();
                if (removedEntities.Count > 0)
                {
                    messageBuilder.AppendLine("Changed Entities:");

                    foreach (var dbEntityEntry in removedEntities)
                    {
                        messageBuilder.AppendFormat("     {0}\r\n", dbEntityEntry.Entity.GetType());
                    }

                    messageBuilder.AppendLine();
                }
            }
        }

        private void StartTrackingCommand(DbCommand command, DbCommandInterceptionContext context)
        {
            if (command == null)
                return;

            try
            {
                var messageBuilder = new StringBuilder(1024);

                string caption;
                if (command.CommandType == CommandType.StoredProcedure)
                {
                    caption = string.Format("Executing '{0}'", command.CommandText);
                }
                else
                {
                    //we want to make a more compact version of the SQL Query for the caption...
                    var queryLines = command.CommandText.Split(new[] {'\r', '\n'});

                    //now rip out any leading/trailing white space...
                    var cleanedUpLines = new List<string>(queryLines.Length);
                    foreach (var queryLine in queryLines)
                    {
                        if (string.IsNullOrWhiteSpace(queryLine) == false)
                        {
                            string minimizedLine = queryLine.Trim();

                            if (string.IsNullOrWhiteSpace(minimizedLine) == false)
                            {
                                cleanedUpLines.Add(minimizedLine);
                            }
                        }
                    }

                    //and rejoin to make the shortened command.
                    var shortenedCommand = string.Join(" ", cleanedUpLines);
                    if (shortenedCommand.Length > 512)
                    {
                        shortenedCommand = shortenedCommand.Substring(0, 512) + "(...)";
                        messageBuilder.AppendFormat("Full Query:\r\n\r\n{0}\r\n\r\n", command.CommandText);
                    }
                    caption = string.Format("Executing Sql: '{0}'", shortenedCommand);
                }

                string paramString = null;
                if (command.Parameters.Count > 0)
                {
                    var parameterValues = new List<string>();

                    foreach (DbParameter parameter in command.Parameters)
                    {
                        parameterValues.Add(string.Format("{0}='{1}'", parameter.ParameterName, parameter.Value));
                    }

                    paramString = String.Join(", ", parameterValues.ToArray());

                    messageBuilder.AppendFormat("Parameters: {0}\r\n\r\n", paramString);
                }

                var trackingMetric = new DatabaseMetric(command.CommandText);
                trackingMetric.Parameters = paramString;

                if (command.Transaction != null)
                {
                    messageBuilder.AppendFormat("Transaction:\r\n    Id: {0:X}\r\n    Isolation Level: {1}\r\n\r\n", command.Transaction.GetHashCode(), command.Transaction.IsolationLevel);
                }

                var connection = command.Connection;
                if (connection != null)
                {
                    messageBuilder.AppendFormat("Server:\r\n    DataSource: {3}\r\n    Command Timeout: {2:N0} Seconds\r\n    Provider: {0}\r\n    Server Version: {1}\r\n\r\n",
                                                connection.GetType(), connection.ServerVersion, connection.ConnectionTimeout, connection.DataSource);
                }

                var messageSourceProvider = new MessageSourceProvider(2); //It's a minimum of two frames to our caller.
                if (IncludeCallStack)
                {
                    messageBuilder.AppendFormat("Call Stack:\r\n{0}\r\n\r\n", messageSourceProvider.StackTrace);
                }


                Log.Write(LogMessageSeverity.Verbose, LogSystem, messageSourceProvider, null, null, LogWriteMode.Queued, null, LogCategory, caption,
                          messageBuilder.ToString());

                //we have to stuff the tracking metric in our index so that we can update it on the flipside.
                try
                {
                    _databaseMetrics[command.GetHashCode()] = trackingMetric;
                }
                catch (Exception ex)
                {
#if DEBUG
                    Log.Error(ex, LogCategory, "Unable to set database tracking metric for command due to " + ex.GetType(), "While storing the database metric for the current operation a {0} was thrown so it's unpredictable what will be recorded at the end of the operation.\r\n{1}", ex.GetType(), ex.Message);
#endif
                    GC.KeepAlive(ex);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Log.Error(ex, LogCategory, "Unable to record Entity Framework event due to " + ex.GetType(), "While calculating the log message for this event a {0} was thrown so we are unable to record the event.\r\n{1}", ex.GetType(), ex.Message);
#endif
                GC.KeepAlive(ex);
            }
        }

        private void StopTrackingCommand(DbCommand command, DbCommandInterceptionContext context, int? result)
        {
            string paramString = null;

            //see if we have a tracking metric for this command...
            DatabaseMetric trackingMetric;
            _databaseMetrics.TryGetValue(command.GetHashCode(), out trackingMetric);
            if (trackingMetric != null)
            {
                trackingMetric.Stop();
                paramString = trackingMetric.Parameters;

                if (result != null)
                {
                    trackingMetric.Result = result.ToString();
                }

                _databaseMetrics.Remove(command.GetHashCode());
            }

            if (context.Exception != null)
            {
                if (trackingMetric != null)
                {
                    trackingMetric.Result = context.Exception.ToString();
                }

                if (LogExceptions)
                {
                    Log.Warning(context.Exception, LogCategory, "Database Call failed due to " + context.Exception.GetType() + ": " + command.CommandText,
                              "Parameters: {0}\r\n\r\nException: {1}", paramString ?? "(none)", context.Exception.Message);
                }
            }

            if (trackingMetric != null)
            {
                trackingMetric.Record();
            }
        }
    }
}
