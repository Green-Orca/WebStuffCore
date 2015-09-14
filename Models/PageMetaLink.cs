using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStuff.Models
{
    public class PageMetaLink
    {
        public string Rel { get; set; }
        public string Type { get; set; }
        public string Href { get; set; }
        public Dictionary<string, string> MiscAttributes = new Dictionary<string, string>();
    }
}
