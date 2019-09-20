// Author: Gockner, Simon
// Created: 2019-09-20
// Copyright(c) 2019 SimonG. All Rights Reserved.

namespace Resharper.AutoFormatOnSave.Interfaces
{
    public interface IAllowedFileExtensionsOptions
    {
        /// <summary>
        /// Is .cs file extension allowed
        /// </summary>
        bool IsCsAllowed { get; set; }

        /// <summary>
        /// Is .xaml file extension allowed
        /// </summary>
        bool IsXamlAllowed { get; set; }

        /// <summary>
        /// Is .vb file extension allowed
        /// </summary>
        bool IsVbAllowed { get; set; }

        /// <summary>
        /// Is .js file extension allowed
        /// </summary>
        bool IsJsAllowed { get; set; }

        /// <summary>
        /// Is .ts file extension allowed
        /// </summary>
        bool IsTsAllowed { get; set; }

        /// <summary>
        /// Is .css file extension allowed
        /// </summary>
        bool IsCssAllowed { get; set; }

        /// <summary>
        /// Is .html file extension allowed
        /// </summary>
        bool IsHtmlAllowed { get; set; }

        /// <summary>
        /// Is .xml file extension allowed
        /// </summary>
        bool IsXmlAllowed { get; set; }
    }
}