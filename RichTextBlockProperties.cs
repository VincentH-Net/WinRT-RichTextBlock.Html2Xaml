using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Data.Xml.Xsl;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;

namespace WinRT_RichTextBlock.Html2Xaml
{
    /// <summary>
    /// Usage: 
    /// 1) In a XAML file, declare the above namespace, e.g.:
    ///    xmlns:rtbx="using:WinRT_RichTextBlock.Html2Xaml"
    ///     
    /// 2) In RichTextBlock controls, set or databind the Html property, e.g.:
    ///    <RichTextBlock rtbx:Properties.Html="{Binding ...}"/>
    ///    or
    ///    <RichTextBlock>
    ///     <rtbx:Properties.Html>
    ///         <![CDATA[
    ///             <p>This is a list:</p>
    ///             <ul>
    ///                 <li>Item 1</li>
    ///                 <li>Item 2</li>
    ///                 <li>Item 3</li>
    ///             </ul>
    ///         ]]>
    ///     </rtbx:Properties.Html>
    /// </RichTextBlock>
    /// </summary>
    public class Properties : DependencyObject
    {
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Properties), new PropertyMetadata(null, HtmlChanged));

        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        private static async void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Get the target RichTextBlock
            RichTextBlock richText = d as RichTextBlock;
            if (richText == null) return;

            // Wrap the value of the Html property in a div and convert it to a new RichTextBlock
            string xhtml = string.Format("<div>{0}</div>", e.NewValue as string);
            string xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
            RichTextBlock newRichText = (RichTextBlock)XamlReader.Load(xaml);

            // Move the blocks in the new RichTextBlock to the target RichTextBlock
            richText.Blocks.Clear();
            for (int i = newRichText.Blocks.Count - 1; i >= 0; i--)
            {
                Block b = newRichText.Blocks[i];
                newRichText.Blocks.RemoveAt(i);
                richText.Blocks.Insert(0, b);
            }
        }

        private static XsltProcessor Html2XamlProcessor;

        private static async Task<string> ConvertHtmlToXamlRichTextBlock(string xhtml)
        {
            // Load XHTML fragment as XML document
            XmlDocument xhtmlDoc = new XmlDocument();
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // In design mode we swallow all exceptions to make editing more friendly
                try { xhtmlDoc.LoadXml(xhtml); }
                catch { } // For some reason code in catch is not executed when an exception occurs in design mode, so we can't display a friendly error here.
            }
            else
            {
                // When not in design mode, we let the application handle any exceptions
                xhtmlDoc.LoadXml(xhtml);
            }

            if (Html2XamlProcessor == null)
            {
                // Read XSLT. In design mode we cannot access the xslt from the file system (with Build Action = Content), 
                // so we use it as an embedded resource instead:
                Assembly assembly = typeof(Properties).GetTypeInfo().Assembly;
                using (Stream stream = assembly.GetManifestResourceStream("WinRT_RichTextBlock.Html2Xaml.RichTextBlockHtml2Xaml.xslt"))
                {
                    StreamReader reader = new StreamReader(stream);
                    string content = await reader.ReadToEndAsync();
                    XmlDocument html2XamlXslDoc = new XmlDocument();
                    html2XamlXslDoc.LoadXml(content);
                    Html2XamlProcessor = new XsltProcessor(html2XamlXslDoc);
                }
            }

            // Apply XSLT to XML
            string xaml = Html2XamlProcessor.TransformToString(xhtmlDoc.FirstChild);
            return xaml;
        }

    }
}
