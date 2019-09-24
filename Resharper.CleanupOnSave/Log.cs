// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

using Microsoft.VisualStudio.Shell.Interop;
using Resharper.CleanupOnSave.Interfaces;

namespace Resharper.CleanupOnSave
{
    public class Log : ILog
    {
        private static IVsOutputWindowPane _outputPane;
        private static ILoggingOptions _loggingOptions;

        private static LogLevel LogLevel => _loggingOptions.LogLevel;

        /// <summary>
        /// Initialize <see cref="Log"/>
        /// </summary>
        /// <param name="outputPane">The Visual Studio output window pane</param>
        /// <param name="loggingOptions">The logging options</param>
        public void InitializeLog(IVsOutputWindowPane outputPane, ILoggingOptions loggingOptions)
        {
            _outputPane = outputPane;
            _loggingOptions = loggingOptions;
        }

        /// <summary>
        /// Write a given <see cref="string"/> with the <see cref="CleanupOnSave.LogLevel.Default"/> to the <see cref="IVsOutputWindowPane"/>
        /// </summary>
        /// <param name="line">The given string</param>
        public static void WriteLine(string line)
        {
            WriteLine(LogLevel.Default, line);
        }

        /// <summary>
        /// Write a given <see cref="string"/> to the <see cref="IVsOutputWindowPane"/>
        /// </summary>
        /// <param name="logLevel">The <see cref="CleanupOnSave.LogLevel"/></param>
        /// <param name="line">The given string</param>
        public static void WriteLine(LogLevel logLevel, string line)
        {
            if (logLevel > LogLevel) //logLevel of the message can't be higher than the set LogLevel
                return;

            _outputPane.OutputString($"{line}\n");
            _outputPane.Activate();
        }

        public void Dispose()
        {
            _outputPane.Hide();
            _outputPane.Clear();
            _outputPane = null;
        }
    }
}