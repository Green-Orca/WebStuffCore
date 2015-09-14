using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStuff.Models
{
    /// <summary>
    /// Defines an Anchor (Hyperlink) tag with attributes and text as fieldnames.
    /// </summary>
    /// <remarks>
    /// The attributes marked with * are the ones introduced in HTML5
    /// The ones with ^ are not supported in HTML5
    /// </remarks>
    public class Anchor
    {
        public string Charset { get; set; }
        public string Href { get; set; }
        public string HrefLang { get; set; }
        public string Title { get; set; }
        public string Target { get; set; }
        public string Rel { get; set; }
        public string Name { get; set; }           // ^ 
        public string Download { get; set; }       // * Specifies that the target will be downloaded when a user clicks on the hyperlink
        public string Media { get; set; }          // * Specifies what media/device the linked document is optimized for
        public string Type { get; set; }           // * Specifies the MIME type of the linked document
        public string Text { get; set; }
    }
}
