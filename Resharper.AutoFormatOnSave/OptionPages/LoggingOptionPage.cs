// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Resharper.AutoFormatOnSave.Interfaces;

namespace Resharper.AutoFormatOnSave.OptionPages
{
    public class LoggingOptionPage : DialogPage, ILoggingOptions
    {
        /// <summary>
        /// The <see cref="AutoFormatOnSave.LogLevel"/> of the extension
        /// </summary>
        [Category("Logging")]
        [DisplayName("LogLevel")]
        [Description("The LogLevel of the extension.")]
        public LogLevel LogLevel { get; set; } = LogLevel.Default;
    }
}