// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

namespace Resharper.CleanupOnSave.Interfaces
{
    public interface ILoggingOptions
    {
        /// <summary>
        /// The <see cref="CleanupOnSave.LogLevel"/> of the extension
        /// </summary>
        LogLevel LogLevel { get; set; }
    }
}