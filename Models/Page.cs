using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using HtmlAgilityPack;

namespace WebStuff.Models
{
    /// <summary>
    /// Defines a page items with the most important bits of an HTML page
    /// </summary>
    /// <remarks></remarks>
    public class Page
    {
        public HttpStatusCode StatusCode { get; set; }
        public string RemoteIP { get; set; }
        public string RequestHeader { get; set; }
        public string ResponseHeader { get; set; }
        public string DocType { get; set; }
        public string Title { get; set; }
        public List<PageMeta> MetaInfo;              //  Contains Meta Title, Desc, OG info
        public List<PageMetaLink> MetaLinkInfo;      //  Contains Canonical, Stylesheet, etc.
        public string URL { get; set; }
        public string AnalyticsCode { get; set; }
        public HtmlDocument Document;
        public List<Anchor> AnchorList;
        public List<Image> ImageList;
        public Page Parent;
        public Dict<string, string> OtherTags;               //  This is to store other, arbitrary tags from the page
        public string LastError { get; set; }
        public List<Page> DirectChildren;

        public Page(Page p_pParentPage) {
            this.Parent = p_pParentPage;
            this.Document = new HtmlDocument();
            this.MetaInfo = new List<PageMeta>();
            this.MetaLinkInfo = new List<PageMetaLink>();
            this.AnchorList = new List<Anchor>();
            this.ImageList = new List<Image>();
            this.OtherTags = new Dict<string, string>();
            this.DirectChildren = new List<Page>();
        }

        public Page() : this(null) { }
    }

}
