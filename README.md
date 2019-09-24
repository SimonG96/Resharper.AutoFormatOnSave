# Resharper.AutoFormatOnSave

![Visual Studio Marketplace Version](https://img.shields.io/visual-studio-marketplace/v/SimonG.ReSharperAutoFormatOnSave?logo=microsoft)
![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/SimonG.ReSharperAutoFormatOnSave?logo=microsoft)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/SimonG.ReSharperAutoFormatOnSave?logo=microsoft)

Resharper.AutoFormatOnSave is a Visual Studio Extension that calls ReSharper's `Silent Cleanup Code` command when you save your solution.  
It is based on [this](https://github.com/PombeirP/ReSharper.AutoFormatOnSave) extension.

## Allowed File Extensions

If you don't want to execute the command for a specific file extension, you can change the allowed file extensions.

To do this you have to open `Tools`->`Options` and go to the category `ReSharper.AutoFormatOnSave`. Under `Allowed File Extensions` you can now disallow a specific file extension by setting it's value to `false`.
