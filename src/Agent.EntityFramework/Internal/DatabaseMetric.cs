#region File Header and License
// /*
//    DatabaseMetric.cs
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
using System;
using System.Diagnostics;
using Gibraltar.Agent.Metrics;

namespace Gibraltar.Agent.EntityFramework.Internal
{
    /// <summary>
    /// The metric data object that implements our database metric.
    /// </summary>
    [EventMetric("Gibraltar", "Database", "Query", Caption = "Database Query Performance",
      Description = "Performance data for every database query")]
    internal class DatabaseMetric
    {
        private readonly Stopwatch _stopWatch;

        /// <summary>
        /// Create a new metric instance for the specified query
        /// </summary>
        public DatabaseMetric(string query)
        {
            Query = query;
            _stopWatch = Stopwatch.StartNew();
            //by default assume we're going to succeed - that way we don't have to explicitly add this
            //to every place we record a metric.
            Result = "Success";
        }

        /// <summary>
        /// The name of the stored procedure or query that was executed
        /// </summary>
        [EventMetricValue("queryName", SummaryFunction.Count, null, Caption = "Query Name",
            Description = "The name of the stored procedure or query that was executed")]
        public string Query { get; private set; }


        /// <summary>
        /// The parameters that were provided as input to the query
        /// </summary>
        [EventMetricValue("parameters", SummaryFunction.Count, null, Caption = "Parameters",
            Description = "The parameters that were provided as input to the query")]
        public string Parameters { get; set; }

        /// <summary>
        /// The number of rows returned by the query
        /// </summary>
        [EventMetricValue("rowCount", SummaryFunction.Average, null, Caption = "Rows",
            Description = "The number of rows returned by the query")]
        public int Rows { get; set; }

        /// <summary>
        /// Duration of the query execution
        /// </summary>
        [EventMetricValue("duration", SummaryFunction.Average, "ms", Caption = "Duration",
            Description = "Duration of the query execution", IsDefaultValue = true)]
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// The result of the query; Success or an error message.
        /// </summary>
        [EventMetricValue("result", SummaryFunction.Count, null, Caption = "Result",
            Description = "The result of the query; Success or an error message.")]
        public string Result { get; set; }

        /// <summary>
        /// Stops the timer but doesn't record the metric yet.
        /// </summary>
        public void Stop()
        {
            if (_stopWatch.IsRunning)
            _stopWatch.Stop();

            Duration = _stopWatch.Elapsed;

            //fix for issue with .Net < 4 where short durations can be negative on some computers.
            if (Duration.Ticks < 0)
                Duration = new TimeSpan(0);
        }

        /// <summary>
        /// Stops the timer and records the metric
        /// </summary>
        public void Record()
        {
            Stop();

            EventMetric.Write(this);
        }
    }
}
