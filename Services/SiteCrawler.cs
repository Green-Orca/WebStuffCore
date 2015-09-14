using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

using HtmlAgilityPack;

using WebStuff.Models;

namespace WebStuff.Services
{
    public class SiteCrawler
    {
        public string m_strSiteDomain = "";
        public string SiteDomain {
            get { return m_strSiteDomain; }
            set { m_strSiteDomain = value; } 
        }

        private List<Page> m_lstGlobalList = new List<Page>();
        public List<Page> GlobalList {
            get { return m_lstGlobalList; }
            set { m_lstGlobalList = value; }
        }

        public Page TraversePages(string p_strURL, Page p_pParentPage = null, bool blnIsRecursive = false)
        {
            //  Grab the site domain on the very first call of the function (the only case when the parent is null)
            //  This works for recursive calls, but not for non-recursive page crawls with a parent, so we need a second condition
            if (p_pParentPage == null || !blnIsRecursive)
            {
                //  Extract scheme + domain info (i.e. http(s)://www.domain.name)
                if (Uri.IsWellFormedUriString(p_strURL, UriKind.Absolute))
                {
                    m_strSiteDomain = UsefulStuff.GrabHostAndScheme(p_strURL);
                }
                else
                {
                    return null;
                }
            }

            //  Create new page instance (even if the parent's null)
            Page pCurrentPage = new Page(p_pParentPage);
            pCurrentPage.URL = p_strURL;

            //  Check if URL is well-formed. If not, create the page and return it with an error.
            if (!Uri.IsWellFormedUriString(p_strURL, UriKind.RelativeOrAbsolute))
            {
                pCurrentPage.LastError = "URL is not well formed.";

                p_strURL = UsefulStuff.CreateWellFormedUrl(p_strURL, m_strSiteDomain);
                if(p_strURL == "")    //  If it's no good, return with an error and break the thread
                {
                    pCurrentPage.LastError += " Could not resolve ill-formedness.";
                    return pCurrentPage;
                }
            }

            if (Uri.IsWellFormedUriString(p_strURL, UriKind.Absolute)) { 
                //  We've got a working URL, let's get on with grabbing more page info
                //  Build up the request to the URL and grab the stream from the response (i.e. the source of the page)
                HttpWebRequest hwrqRequest = (HttpWebRequest)WebRequest.Create(p_strURL);
                hwrqRequest.AllowAutoRedirect = false;  //  Switch off AutoRedirect so that we can capture 301s & 302s

                WebResponse wrpResponse = null;
                
                //  Getting a 404 throws a WebException so handle that here.
                try
                {
                    wrpResponse = hwrqRequest.GetResponse();
                }
                catch (WebException wex) {
                    wrpResponse = (WebResponse)wex.Response;
                    pCurrentPage.StatusCode = HttpStatusCode.NotFound;
                    pCurrentPage.LastError = wex.ToString();
                }
                catch (Exception ex)
                {
                    pCurrentPage.LastError = ex.ToString();
                    return pCurrentPage;
                }

                //  If we've got a response, continue grabbing details
                if (wrpResponse != null)
                {

                    HttpWebResponse hwrpResponse = (HttpWebResponse)wrpResponse;

                    //  Add status code to page properties
                    pCurrentPage.StatusCode = hwrpResponse.StatusCode;

                    //  Check if the request returns with a 200 status
                    //  If the status is cool, carry on by analysing the page data with HtmlAgility
                    if (hwrpResponse.StatusCode == HttpStatusCode.OK)
                    {
                        //  Grab page info from HTML page contents
                        using (Stream stmPageStream = wrpResponse.GetResponseStream())
                        {
                            using (StreamReader srdrPageReader = new StreamReader(stmPageStream))
                            {
                                HtmlDocument htmlDocDocument = new HtmlDocument();
                                htmlDocDocument.Load(srdrPageReader);

                                string strPageHtmlContent = srdrPageReader.ReadToEnd();//.Replace("\n", "").Replace("\r", "");         // Replace all new lines so that we can do singleline matches to make our life easier
                                pCurrentPage.Document = htmlDocDocument;
                                pCurrentPage.DocType = GetDocType(strPageHtmlContent);
                                pCurrentPage.Title = GetPageTitle(htmlDocDocument);
                                pCurrentPage.MetaInfo = GetPageMetaInfo(htmlDocDocument);
                                pCurrentPage.MetaLinkInfo = GetPageMetaLinkInfo(htmlDocDocument);
                                pCurrentPage.AnchorList = GetAnchorList(htmlDocDocument);
                                pCurrentPage.ImageList = GetImageList(htmlDocDocument);
                                pCurrentPage.URL = UsefulStuff.CreateWellFormedUrl(p_strURL, m_strSiteDomain);

                                m_lstGlobalList.Add(pCurrentPage);
                                if (p_pParentPage != null) p_pParentPage.DirectChildren.Add(pCurrentPage);

                                //  If we're getting the page recursively (i.e. all the links on page as well) and we've got a list of anchors,
                                //  then get on and grab the stuff
                                if (blnIsRecursive && pCurrentPage.AnchorList != null)
                                {
                                    //  Loop through 
                                    foreach (Anchor anchor in pCurrentPage.AnchorList)
                                    {
                                        //  Check if we're staying on the site, otherwise we'll grab the whole internet ;)
                                        string strWellFormedHref = UsefulStuff.CreateWellFormedUrl(anchor.Href, m_strSiteDomain);
                                        if (UsefulStuff.GrabHostAndScheme(strWellFormedHref) == m_strSiteDomain)
                                        {
                                            //  Check if the page has been traversed before, ignore if so
                                            int pageCount = m_lstGlobalList.Count(x => x.URL == strWellFormedHref);
                                            if (pageCount < 1)
                                            {
                                                TraversePages(strWellFormedHref, pCurrentPage, true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //  Close response objects
                    hwrpResponse.Close();
                    wrpResponse.Close();
                }
            }

            return pCurrentPage;
        }

        public static List<Page> ConvertTreeToListOfPages(Page p_pgRootPage) {
            List<Page> lstPgAllPages = new List<Page>();
            lstPgAllPages.Add(p_pgRootPage);
            foreach (Page page in p_pgRootPage.DirectChildren) {
                AddPageToCollection(page, lstPgAllPages);
            }
            return lstPgAllPages;
        }

        /// <summary>
        /// Recursive method for adding a page and its direct children to a collection (ref type)
        /// </summary>
        /// <param name="p_pgPage"></param>
        /// <param name="p_lstPgAllPages"></param>
        private static void AddPageToCollection(Page p_pgPage, List<Page> p_lstPgAllPages)
        {
            p_lstPgAllPages.Add(p_pgPage);
            foreach (Page page in p_pgPage.DirectChildren)
            {
                AddPageToCollection(page, p_lstPgAllPages);
            }
        }


    #region HTML Attribute Getters (HtmlAgilityPack)

        /// <summary>
        /// Extracts anchor information using HtmlDocument (Html Agility Pack)
        /// </summary>
        /// <param name="htmlDocDocument"></param>
        /// <returns></returns>
        private static List<Anchor> GetAnchorList(HtmlDocument p_htmlDocDocument)
        {
            List<Anchor> ancrLstReturnList = new List<Anchor>();

            if (p_htmlDocDocument.DocumentNode.SelectNodes("//a[@href]") != null)
            {
                foreach (HtmlNode hnLink in p_htmlDocDocument.DocumentNode.SelectNodes("//a[@href]"))
                {
                    Anchor aNewAnchorTag = new Anchor();
                    aNewAnchorTag.Href = GetHtmlAttributeValue(hnLink.Attributes, "href");
                    aNewAnchorTag.HrefLang = GetHtmlAttributeValue(hnLink.Attributes, "hreflang");
                    aNewAnchorTag.Title = GetHtmlAttributeValue(hnLink.Attributes, "title");
                    aNewAnchorTag.Target = GetHtmlAttributeValue(hnLink.Attributes, "target");
                    aNewAnchorTag.Rel = GetHtmlAttributeValue(hnLink.Attributes, "rel");
                    aNewAnchorTag.Name = GetHtmlAttributeValue(hnLink.Attributes, "name");
                    aNewAnchorTag.Download = GetHtmlAttributeValue(hnLink.Attributes, "download");
                    aNewAnchorTag.Media = GetHtmlAttributeValue(hnLink.Attributes, "media");
                    aNewAnchorTag.Type = GetHtmlAttributeValue(hnLink.Attributes, "type");
                    aNewAnchorTag.Text = hnLink.InnerText;

                    ancrLstReturnList.Add(aNewAnchorTag);
                }
            }
            return ancrLstReturnList;
        }

        /// <summary>
        /// Extracts image information using HtmlDocument (Html Agility Pack)
        /// </summary>
        /// <param name="htmlDocDocument"></param>
        /// <returns></returns>
        private static List<Image> GetImageList(HtmlDocument p_htmlDocDocument)
        {
            List<Image> ancrLstReturnList = new List<Image>();

            if (p_htmlDocDocument.DocumentNode.SelectNodes("//img[@src]") != null)
            {
                foreach (HtmlNode hnLink in p_htmlDocDocument.DocumentNode.SelectNodes("//img[@src]"))
                {
                    Image imgNewImageTag = new Image();
                    imgNewImageTag.Align = GetHtmlAttributeValue(hnLink.Attributes, "align");
                    imgNewImageTag.Alt = GetHtmlAttributeValue(hnLink.Attributes, "alt");
                    imgNewImageTag.Border = GetHtmlAttributeValue(hnLink.Attributes, "border");
                    imgNewImageTag.Crossorigin = GetHtmlAttributeValue(hnLink.Attributes, "crossorigin");
                    imgNewImageTag.Height = GetHtmlAttributeValue(hnLink.Attributes, "height");
                    imgNewImageTag.Hspace = GetHtmlAttributeValue(hnLink.Attributes, "hspace");
                    imgNewImageTag.Ismap = GetHtmlAttributeValue(hnLink.Attributes, "ismap");
                    imgNewImageTag.Longdesc = GetHtmlAttributeValue(hnLink.Attributes, "longdesc");
                    imgNewImageTag.Src = GetHtmlAttributeValue(hnLink.Attributes, "src");
                    imgNewImageTag.Title = GetHtmlAttributeValue(hnLink.Attributes, "title");
                    imgNewImageTag.Usemap = GetHtmlAttributeValue(hnLink.Attributes, "usemap");
                    imgNewImageTag.Vspace = GetHtmlAttributeValue(hnLink.Attributes, "vspace");
                    imgNewImageTag.Width = GetHtmlAttributeValue(hnLink.Attributes, "width");

                    ancrLstReturnList.Add(imgNewImageTag);
                }
            }
            return ancrLstReturnList;
        }

        /// <summary>
        /// Grabs the page title from the HTML Document passed in
        /// </summary>
        /// <param name="htmlDocDocument"></param>
        /// <returns></returns>
        private static string GetPageTitle(HtmlDocument p_htmlDocDocument)
        {
            string strTitleOutput = "";

            if (p_htmlDocDocument.DocumentNode.SelectNodes("//title") != null)
            {
                foreach (HtmlNode hnTitle in p_htmlDocDocument.DocumentNode.SelectNodes("//title"))
                {
                    strTitleOutput = hnTitle.InnerText;
                }
            }

            return strTitleOutput;
        }

        /// <summary>
        /// Grabs and returns Link tags from the page head
        /// </summary>
        /// <param name="htmlDocDocument"></param>
        /// <returns></returns>
        private static List<PageMetaLink> GetPageMetaLinkInfo(HtmlDocument p_htmlDocDocument)
        {
            PageMetaLink pmlLink = null;
            List<PageMetaLink> lstLink = new List<PageMetaLink>();

            if (p_htmlDocDocument.DocumentNode.SelectNodes("//link") != null)
            {
                foreach (HtmlNode hnItem in p_htmlDocDocument.DocumentNode.SelectNodes("//link"))
                {
                    pmlLink = new PageMetaLink();
                    pmlLink.Rel = GetHtmlAttributeValue(hnItem.Attributes, "rel");
                    pmlLink.Href = GetHtmlAttributeValue(hnItem.Attributes, "href");
                    pmlLink.Type = GetHtmlAttributeValue(hnItem.Attributes, "type");
                    lstLink.Add(pmlLink);
                }
            }

            return lstLink;
        }

        /// <summary>
        /// Grabs and returns Meta tags from the page head
        /// </summary>
        /// <param name="htmlDocDocument"></param>
        /// <returns></returns>
        private static List<PageMeta> GetPageMetaInfo(HtmlDocument p_htmlDocDocument)
        {
            PageMeta pmMeta = null;
            List<PageMeta> lstMeta = new List<PageMeta>();

            if (p_htmlDocDocument.DocumentNode.SelectNodes("//meta") != null)
            {
                foreach (HtmlNode hnItem in p_htmlDocDocument.DocumentNode.SelectNodes("//meta"))
                {
                    pmMeta = new PageMeta();
                    pmMeta.Name = GetHtmlAttributeValue(hnItem.Attributes, "name");
                    pmMeta.Property = GetHtmlAttributeValue(hnItem.Attributes, "property");
                    pmMeta.Content = GetHtmlAttributeValue(hnItem.Attributes, "content");
                    lstMeta.Add(pmMeta);
                }
            }

            return lstMeta;
        }

        /// <summary>
        /// Grabs and returns generic tags as (safe) dictionaries from an HTML document - for futureproving
        /// </summary>
        /// <param name="htmlDocDocument"></param>
        /// <returns></returns>
        private static Dict<string, string> GetGenericTag(HtmlDocument p_htmlDocDocument, string p_strTagName)
        {
            Dict<string, string> dictTag = new Dict<string, string>();

            foreach (HtmlNode hnItem in p_htmlDocDocument.DocumentNode.SelectNodes("//" + p_strTagName))
            {
                foreach (HtmlAttribute haItem in hnItem.Attributes)
                {
                    //  Generate name-value pair of the tag's attribute
                    dictTag.Add(haItem.Name, GetHtmlAttributeValue(hnItem.Attributes, haItem.Name));
                }
            }

            return dictTag;
        }

        /// <summary>
        /// Gets the value of an HTML attribute safely from an HTMLAttributeCollection
        /// </summary>
        /// <param name="p_hacCollection"></param>
        /// <param name="p_strItem"></param>
        /// <returns></returns>
        private static string GetHtmlAttributeValue(HtmlAttributeCollection p_hacCollection, string p_strItem)
        {
            if (p_hacCollection.Contains(p_strItem)) return ((HtmlAttribute)p_hacCollection[p_strItem]).Value;
            else return "";
        }

    #endregion

    #region Deprecated RegEx Getters
        /// <summary>
        /// Extracts the meta tag info from the page using RegEx
        /// </summary>
        /// <param name="p_strPageHtmlContent"></param>
        /// <returns>Deprecated</returns>
        private static Dict<string, PageMeta> GetPageMetaInfo(string p_strPageHtmlContent)
        {
            Dict<string, PageMeta> dictReturnSet = new Dict<string, PageMeta>();
            PageMeta pmMetaTag = new PageMeta();

            //   Try grabbing the meta info of the page into a dictionary
            string pattern = "<meta.+?(?:name=(?:\"|')(.*?)(?:\"|').*?)?(?:property=(?:\"|')(.*?)(?:\"|').*?)?(?:content=(?:\"|')(.*?)(?:\"|'))?/?>.*?</head>";
            RegexOptions rxoOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            foreach (Match match in Regex.Matches(p_strPageHtmlContent, pattern, rxoOptions))
            {
                pmMetaTag = new PageMeta();
                pmMetaTag.Name = match.Groups[1].Value;
                pmMetaTag.Property = match.Groups[2].Value;
                pmMetaTag.Content = match.Groups[3].Value;
                if (!dictReturnSet.ContainsKey(match.Groups[1].Value))
                {
                    dictReturnSet.Add(match.Groups[1].Value, pmMetaTag);
                }
            }

            return dictReturnSet;
        }

        /// <summary>
        /// Extracts the link tag info from the page using RegEx
        /// </summary>
        /// <param name="p_strPageHtmlContent"></param>
        /// <returns>Deprecated</returns>
        private static Dict<string, PageMetaLink> GetPageMetaLinkInfo(string p_strPageHtmlContent)
        {
            Dict<string, PageMetaLink> dictReturnSet = new Dict<string, PageMetaLink>();
            PageMetaLink pmlLinkTag = new PageMetaLink();

            //   Try grabbing the meta info of the page into a dictionary
            string pattern = "<link.+?(?:rel=(?:\"|')(.*?)(?:\"|').*?)?(?:type=(?:\"|')(.*?)(?:\"|').*?)?(?:href=(?:\"|')(.*?)(?:\"|'))?/?>.*?</head>";
            RegexOptions rxoOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            foreach (Match match in Regex.Matches(p_strPageHtmlContent, pattern, rxoOptions))
            {
                pmlLinkTag = new PageMetaLink();
                pmlLinkTag.Rel = match.Groups[1].Value;
                pmlLinkTag.Type = match.Groups[2].Value;
                pmlLinkTag.Href = match.Groups[3].Value;
                dictReturnSet.Add(match.Groups[1].Value, pmlLinkTag);
            }

            return dictReturnSet;
        }

        /// <summary>
        /// Extracts DOCTYPE info from the top of the page using RegEx
        /// </summary>
        /// <param name="p_strPageHtmlContent"></param>
        /// <returns>Deprecated</returns>
        private static string GetDocType(string p_strPageHtmlContent)
        {
            string strReturnString = "";

            //   Try grabbing the doctype info of the page
            string pattern = "<!doctype.?(.*?)>";
            RegexOptions rxoOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            foreach (Match match in Regex.Matches(p_strPageHtmlContent, pattern, rxoOptions))
            {
                strReturnString = match.Groups[1].Value;
            }

            return strReturnString;
        }

        /// <summary>
        /// Extracts title tag info from the top of the page using RegEx
        /// </summary>
        /// <param name="p_strPageHtmlContent"></param>
        /// <returns>Deprecated</returns>
        private static string GetPageTitle(string p_strPageHtmlContent)
        {
            string strReturnString = "";

            //   Try grabbing the doctype info of the page
            string pattern = "<title.*?>(.*?)</title>";
            RegexOptions rxoOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            foreach (Match match in Regex.Matches(p_strPageHtmlContent, pattern, rxoOptions))
            {
                strReturnString = match.Groups[1].Value;
            }

            return strReturnString;
        }

        /// <summary>
        /// Extracts the link tag info from the page using RegEx
        /// </summary>
        /// <param name="p_strPageHtmlContent"></param>
        /// <returns>Deprecated</returns>
        private static Dict<string, Anchor> GetAnchorsList(string p_strPageHtmlContent)
        {
            Dict<string, Anchor> dictReturnSet = new Dict<string, Anchor>();
            Anchor ancrLink = new Anchor();

            //   Try grabbing the meta info of the page into a dictionary
            string pattern = "<a.+?(?:href=(?:\"|')(.*?)(?:\"|').*?)?(?:title=(?:\"|')(.*?)(?:\"|').*?)?(?:href=(?:\"|')(.*?)(?:\"|'))?/?>.*?</head>";
            RegexOptions rxoOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

            foreach (Match match in Regex.Matches(p_strPageHtmlContent, pattern, rxoOptions))
            {
                ancrLink = new Anchor();
                ancrLink.Rel = match.Groups[1].Value;
                ancrLink.Type = match.Groups[2].Value;
                ancrLink.Href = match.Groups[3].Value;
                dictReturnSet.Add(match.Groups[1].Value, ancrLink);
            }

            return dictReturnSet;
        }
        #endregion

    }
}
