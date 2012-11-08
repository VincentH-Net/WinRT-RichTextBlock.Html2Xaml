Dear user,

Thanks for installing the RichTextBlock.Html2Xaml NuGet package.

The WinRT RichTextBlock control serves to display read-only rich formatted text.
However, it supports a limited subset of XAML, and no HTML.

In scenario's where you want to display a field that contains HTML rich 
formatted text (e.g. when you have clients on multiple platforms, such as 
web, WinRT, iOS and Android), this package offers an alternative to 
using an embedded browser control. Using a browser control impacts 
performance and limits the UI experience (e.g. the WebView 
control does not support transparency).

The RichTextBlock.Html2Xaml NuGet package adds an Html extension property
to RichTextBlock controls, that you can set (or data bind) to a string
containing an Html snippet. The Html extension property is implemented in two simple 
source files that can be modified easily: one C# and one XSLT source file, 
no binaries.

The RichTextBlock control only supports a limited set of XAML; this package
supports translating an equivalent limited set of HTML markup via an XSD. 
The XSD allows for simple modification or extension of the XAML to HTML
conversion. Any unsupported HTML tags will be discarded, but their text content
will be displayed.

See https://github.com/MacawNL/WinRT-RichTextBlock.Html2Xaml/ 
for source code and documentation.

NJoy rich text in your Windows 8 store apps!

Vincent Hoogendoorn [Macaw]
