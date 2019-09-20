// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Resharper.AutoFormatOnSave
{
    public class Log : ILog
    {
        private static IVsOutputWindowPane _outputPane;

        public void InitializeLog(IVsOutputWindowPane outputPane)
        {
            _outputPane = outputPane;
        }

        public static void WriteLine(string message)
        {
            _outputPane.OutputString($"{message}\n");
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