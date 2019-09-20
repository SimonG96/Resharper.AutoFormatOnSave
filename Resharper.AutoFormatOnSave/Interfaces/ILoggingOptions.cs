// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

namespace Resharper.AutoFormatOnSave.Interfaces
{
    public interface ILoggingOptions
    {
        /// <summary>
        /// The <see cref="AutoFormatOnSave.LogLevel"/> of the extension
        /// </summary>
        LogLevel LogLevel { get; set; }
    }
}