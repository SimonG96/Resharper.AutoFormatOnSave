// Author: Gockner, Simon
// Created: 2019-09-18
// Copyright(c) 2019 SimonG. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace Resharper.CleanupOnSave
{
    internal static class NativeWindowMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    }
}