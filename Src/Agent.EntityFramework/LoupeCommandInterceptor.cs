using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
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

        private void StartTrackingCommand(DbCommand command, DbCommandInterceptionContext context)
        {
            if (command == null)
                return;

            string paramString = null;
            if (command.Parameters.Count > 0)
            {
                var parameterValues = new List<string>();

                foreach (DbParameter parameter in command.Parameters)
                {
                    parameterValues.Add(string.Format("{0}='{1}'", parameter.ParameterName, parameter.Value));
                }

                paramString = String.Join(", ", parameterValues.ToArray());
            }

            var trackingMetric = new DatabaseMetric(command.CommandText);
            trackingMetric.Parameters = paramString;

            if (IncludeCallStack)
            {
                Log.Write(LogMessageSeverity.Verbose, LogSystem, 2, null, LogWriteMode.Queued, null, LogCategory, "Database Query Starting: " + command.CommandText,
                            "Parameters: {0}\r\n\r\nCall Stack: {1}", paramString ?? "(none)", new StackTrace(2, true));
            }
            else
            {
                Log.Write(LogMessageSeverity.Verbose, LogSystem, 2, null, LogWriteMode.Queued, null, LogCategory, "Database Query Starting: " + command.CommandText,
                            "Parameters: {0}", paramString ?? "(none)");
            }

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
