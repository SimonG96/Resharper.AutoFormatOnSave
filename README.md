# Resharper.CleanupOnSave

![Visual Studio Marketplace Version](https://img.shields.io/visual-studio-marketplace/v/SimonG.ReSharperCleanupOnSave?logo=microsoft)
![Visual Studio Marketplace Downloads](https://img.shields.io/visual-studio-marketplace/d/SimonG.ReSharperCleanupOnSave?logo=microsoft)
![Visual Studio Marketplace Installs](https://img.shields.io/visual-studio-marketplace/i/SimonG.ReSharperCleanupOnSave?logo=microsoft)

Resharper.CleanupOnSave is a Visual Studio Extension that calls ReSharper's `Silent Cleanup Code` command when you save your solution.  
It is based on [this](https://github.com/PombeirP/ReSharper.AutoFormatOnSave) extension.

## Allowed File Extensions

If you don't want to execute the command for a specific file extension, you can change the allowed file extensions.

To do this you have to open `Tools`->`Options` and go to the category `ReSharper.CleanupOnSave`. Under `Allowed File Extensions` you can now disallow a specific file extension by setting it's value to `false`.
