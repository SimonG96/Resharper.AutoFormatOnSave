// Author: Gockner, Simon
// Created: 2019-09-19
// Copyright(c) 2019 SimonG. All Rights Reserved.

using System.ComponentModel;
using Microsoft.VisualStudio.Shell;
using Resharper.AutoFormatOnSave.Interfaces;

namespace Resharper.AutoFormatOnSave.OptionPages
{
    public class AllowedFileExtensionsOptionPage : DialogPage, IAllowedFileExtensionsOptions
    {
        /// <summary>
        /// Is .cs file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".cs")]
        [Description("Is .cs allowed?")]
        public bool IsCsAllowed { get; set; } = true;

        /// <summary>
        /// Is .xaml file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".xaml")]
        [Description("Is .xaml allowed?")]
        public bool IsXamlAllowed { get; set; } = true;

        /// <summary>
        /// Is .vb file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".vb")]
        [Description("Is .vb allowed?")]
        public bool IsVbAllowed { get; set; } = true;

        /// <summary>
        /// Is .js file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".js")]
        [Description("Is .js allowed?")]
        public bool IsJsAllowed { get; set; } = true;

        /// <summary>
        /// Is .ts file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".ts")]
        [Description("Is .ts allowed?")]
        public bool IsTsAllowed { get; set; } = true;

        /// <summary>
        /// Is .css file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".css")]
        [Description("Is .css allowed?")]
        public bool IsCssAllowed { get; set; } = true;

        /// <summary>
        /// Is .html file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".html")]
        [Description("Is .html allowed?")]
        public bool IsHtmlAllowed { get; set; } = true;

        /// <summary>
        /// Is .xml file extension allowed
        /// </summary>
        [Category("Allowed File Extensions")]
        [DisplayName(".xml")]
        [Description("Is .xml allowed?")]
        public bool IsXmlAllowed { get; set; } = true;
    }
}