﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.VisualStudio.TestPlatform.CommandLine.Publisher
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestPlatform.ObjectModel;
    using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
    using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers.Interfaces;

#if NET451
    using Microsoft.VisualStudio.Telemetry;
#endif

    /// <summary>
    /// The metrics publisher.
    /// </summary>
    public class MetricsPublisher : IMetricsPublisher
    {
#if NET451
        private TelemetrySession session;
#endif
        public MetricsPublisher()
        {
#if NET451
            try
            {
                this.session = TelemetryService.DefaultSession;
                this.session.IsOptedIn = true;
                this.session.Start();
            }
            catch (Exception e)
            {
                if (EqtTrace.IsWarningEnabled)
                {
                    EqtTrace.Warning(string.Format(CultureInfo.InvariantCulture, "TelemetrySession: Error in starting Telemetry session : {0}", e.Message));
                }
            }
#endif
        }

        /// <summary>
        /// Publishes the metrics
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="metrics"></param>
        public void PublishMetrics(string eventName, IDictionary<string, object> metrics)
        {
#if NET451
            if (metrics == null || metrics.Count == 0)
            {
                return;
            }

            if (EqtTrace.IsVerboseEnabled)
            {
                EqtTrace.Verbose("TelemetrySession: Sending the telemetry data to the server.");
                EqtTrace.Verbose("Telemetry Data");

                foreach (var metric in metrics)
                {
                    EqtTrace.Verbose("Telemetry Key: {0} and Value: {1}", metric.Key, metric.Value);
                }
            }

            try
            {
                var finalMetrics = RemoveInvalidCharactersFromProperties(metrics);

                TelemetryEvent telemetryEvent = new TelemetryEvent(eventName);

                foreach (var metric in finalMetrics)
                {
                    telemetryEvent.Properties[metric.Key] = metric.Value;
                }

                this.session.PostEvent(telemetryEvent);

                // Log to Text File
                var logEnabled = Environment.GetEnvironmentVariable("VSTEST_LOGTELEMETRY");
                if (!string.IsNullOrEmpty(logEnabled) && logEnabled.Equals("1", StringComparison.Ordinal))
                {
                    this.LogToFile(eventName, finalMetrics, new FileHelper());
                }
            }
            catch (Exception e)
            {
                if (EqtTrace.IsWarningEnabled)
                {
                    EqtTrace.Warning(string.Format(CultureInfo.InvariantCulture, "TelemetrySession: Error in Posting Event: {0}", e.Message));
                }
            }
#endif
        }

        /// <summary>
        /// Dispose the Telemetry Session
        /// </summary>
        public void Dispose()
        {
#if NET451
            try
            {
                this.session.Dispose();
            }
            catch (Exception e)
            {
                if (EqtTrace.IsWarningEnabled)
                {
                    EqtTrace.Warning(string.Format(CultureInfo.InvariantCulture, "TelemetrySession: Error in Disposing Event: {0}", e.Message));
                }
            }
#endif
        }

        /// <summary>
        /// Removes the invalid characters from the properties which are not supported by VsTelemetryAPI's
        /// </summary>
        /// <param name="metrics">
        /// The metrics.
        /// </param>
        /// <returns>
        /// Removes the invalid keys from the Keys
        /// </returns>
        internal IDictionary<string, object> RemoveInvalidCharactersFromProperties(IDictionary<string, object> metrics)
        {
            if (metrics == null)
            {
                return new Dictionary<string, object>();
            }

            var finalMetrics = new Dictionary<string, object>();
            foreach (var metric in metrics)
            {
                if (metric.Key.Contains(":"))
                {
                    var invalidKey = metric.Key;
                    var validKey = invalidKey.Replace(":", string.Empty);
                    finalMetrics.Add(validKey, metric.Value);
                }
                else
                {
                    finalMetrics.Add(metric.Key, metric.Value);
                }
            }

            return finalMetrics;
        }

        /// <summary>
        /// Log the telemetry to file.
        /// For Testing purposes.
        /// </summary>
        /// <param name="eventName">
        /// The event Name.
        /// </param>
        /// <param name="metrics">
        /// Metrics
        /// </param>
        /// <param name="fileHelper">
        /// The file Helper.
        /// </param>
        internal void LogToFile(string eventName, IDictionary<string, object> metrics, IFileHelper fileHelper)
        {
            string resultDirectory = Path.GetTempPath() + "TelemetryLogs";
            string resultFileName = Guid.NewGuid().ToString();
            string path = Path.Combine(resultDirectory, resultFileName);

            if (!fileHelper.DirectoryExists(resultDirectory))
            {
                fileHelper.CreateDirectory(resultDirectory);
            }

            var telemetryData = string.Join(";", metrics.Select(x => x.Key + "=" + x.Value));
            var finalData = string.Concat(eventName, ";", telemetryData);

            fileHelper.WriteAllTextToFile(path, finalData);
        }
    }
}