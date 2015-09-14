using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStuff.Models
{
    public class PageMeta
    {
        public string Name { get; set; }
        public string Property { get; set; }
        public string Content { get; set; }
        Dictionary<string, string> MiscAttributes = new Dictionary<string,string>();
    }
}
