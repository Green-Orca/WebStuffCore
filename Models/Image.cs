using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStuff.Models
{
    /// <summary>
    /// Defines an Image tag with all valid attributes as per 21/05/2014.
    /// </summary>
    /// <remarks>
    /// The attributes marked with * are the ones introduced in HTML5
    /// The ones with ^ are not supported in HTML5
    /// </remarks>
    public class Image
    {
        public string Align { get; set; }          // ^ 
        public string Alt { get; set; }
        public string Border { get; set; }         // ^ 
        public string Crossorigin { get; set; }    // * Allow images from third-party sites that allow cross-origin access to be used with canvas
        public string Height { get; set; }
        public string Hspace { get; set; }         // ^ 
        public string Ismap { get; set; }
        public string Longdesc { get; set; }       // ^ 
        public string Src { get; set; }
        public string Title { get; set; }
        public string Usemap { get; set; }
        public string Vspace { get; set; }         // ^ 
        public string Width { get; set; }
    }
}
