// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

using System;
using Microsoft.VisualStudio.Shell.Interop;

namespace Resharper.AutoFormatOnSave
{
    public interface ILog : IDisposable
    {
        void InitializeLog(IVsOutputWindowPane outputPane);
    }
}