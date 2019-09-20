// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

using Microsoft.VisualStudio.Shell.Interop;

namespace Resharper.AutoFormatOnSave
{
    public class Log : ILog
    {
        private static IVsOutputWindowPane _outputPane;

        /// <summary>
        /// Initialize <see cref="Log"/>
        /// </summary>
        /// <param name="outputPane">The Visual Studio output window pane</param>
        public void InitializeLog(IVsOutputWindowPane outputPane)
        {
            _outputPane = outputPane;
        }

        /// <summary>
        /// Write a given <see cref="string"/> to the <see cref="IVsOutputWindowPane"/>
        /// </summary>
        /// <param name="line">The given string</param>
        public static void WriteLine(string line)
        {
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