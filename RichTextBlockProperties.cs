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
    ///    xmlns:common="using:WinRT_RichTextBlock.Html2Xaml"
    ///     
    /// 2) In RichTextBlock controls, set or databind the Html property, e.g.:
    /// 
    ///    <RichTextBlock common:Properties.Html="{Binding ...}"/>
    ///    
    ///    or
    ///    
    ///    <RichTextBlock>
    ///       <common:Properties.Html>
    ///         <![CDATA[
    ///             <p>This is a list:</p>
    ///             <ul>
    ///                 <li>Item 1</li>
    ///                 <li>Item 2</li>
    ///                 <li>Item 3</li>
    ///             </ul>
    ///         ]]>
    ///       </common:Properties.Html>
    ///    </RichTextBlock>
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
            RichTextBlock newRichText = null;
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                // In design mode we swallow all exceptions to make editing more friendly
                string xaml = "";
                try {
                    xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
                    newRichText = (RichTextBlock)XamlReader.Load(xaml);
                }
                catch (Exception ex) {
                    string errorxaml = string.Format(@"
                        <RichTextBlock 
                         xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                         xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                        >
                            <Paragraph>An exception occurred while converting HTML to XAML: {0}</Paragraph>
                            <Paragraph />
                            <Paragraph>HTML:</Paragraph>
                            <Paragraph>{1}</Paragraph>
                            <Paragraph />
                            <Paragraph>XAML:</Paragraph>
                            <Paragraph>{2}</Paragraph>
                        </RichTextBlock>",
                        ex.Message,
                        EncodeXml(xhtml),
                        EncodeXml(xaml)
                    );
                    newRichText = (RichTextBlock)XamlReader.Load(errorxaml);
                } // Display a friendly error in design mode.
            }
            else
            {
                // When not in design mode, we let the application handle any exceptions
                string xaml = await ConvertHtmlToXamlRichTextBlock(xhtml);
                newRichText = (RichTextBlock)XamlReader.Load(xaml);
            }

            // Move the blocks in the new RichTextBlock to the target RichTextBlock
            richText.Blocks.Clear();
            if (newRichText != null)
            {
                for (int i = newRichText.Blocks.Count - 1; i >= 0; i--)
                {
                    Block b = newRichText.Blocks[i];
                    newRichText.Blocks.RemoveAt(i);
                    richText.Blocks.Insert(0, b);
                }
            }
        }

        private static string EncodeXml(string xml)
        {
            string encodedXml = xml.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;");
            return encodedXml;
        }

        private static XsltProcessor Html2XamlProcessor;

        private static async Task<string> ConvertHtmlToXamlRichTextBlock(string xhtml)
        {
            // Load XHTML fragment as XML document
            XmlDocument xhtmlDoc = new XmlDocument();
            xhtmlDoc.LoadXml(xhtml);

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
