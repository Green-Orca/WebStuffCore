using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStuff.Models
{
    public class Tag
    {
        public string Name { get; set; }
        public Dict<string, string> Attributes { get; set; }
        public string Contents { get; set; }
    }
}
