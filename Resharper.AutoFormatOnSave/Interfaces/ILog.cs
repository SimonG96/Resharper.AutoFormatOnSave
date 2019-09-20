// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Resharper.AutoFormatOnSave.Interfaces
{
    public interface ILog : IDisposable
    {
        /// <summary>
        /// Initialize <see cref="ILog"/>
        /// </summary>
        /// <param name="outputPane">The Visual Studio output window pane</param>
        /// <param name="loggingOptions">The logging options</param>
        void InitializeLog(IVsOutputWindowPane outputPane, ILoggingOptions loggingOptions);
    }
}