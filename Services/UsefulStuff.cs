using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebStuff.Services
{
    public class UsefulStuff
    {

    #region URL Manipulation
        /// <summary>
        /// Extracts the scheme + host information from a URL (i.e. http(s)://www.domain.name)
        /// </summary>
        /// <param name="p_strUrl">The URL to be broken up as a URL - needs to be absolute</param>
        /// <returns>The scheme and host name as a string</returns>
        public static string GrabHostAndScheme(string p_strUrl) {
            try
            {
                Uri uriReturnUri = new Uri(p_strUrl, UriKind.Absolute);
                return uriReturnUri.Scheme + "://" + uriReturnUri.Host;
            }
            catch (Exception) { return ""; }
        }

        /// <summary>
        /// Checks a URL and creates a well-formed, absolute URL adding the scheme and host if necessary
        /// </summary>
        /// <param name="p_strUrl">The URL to be checked</param>
        /// <param name="p_strDomain">The host and scheme information to be added if necessary</param>
        /// <returns>The well-formed URL as a string, the original URL if no modifications or empty string if it's not useable.</returns>
        public static string CreateWellFormedUrl(string p_strUrl, string p_strDomain, bool p_blnRemoveDifferingDomain = false) {

            //  In case nothing needs to be changed, return the original URL
            string strToReturn = p_strUrl;

            //  Append http if URL is absolute and doesn't have it already.
            if (!Uri.IsWellFormedUriString(p_strUrl, UriKind.Absolute))
            {
                if (Uri.IsWellFormedUriString(p_strUrl, UriKind.Relative))   //  Append host if URL is relative
                {
                    //  Check for exceptions:
                    //    * javascript function
                    //    * email, tel
                    //    * #
                    if (p_strUrl.StartsWith("javascript:")
                        || p_strUrl.StartsWith("mailto:")
                        || p_strUrl.StartsWith("tel:")
                        || p_strUrl.StartsWith("#"))
                        strToReturn = "";
                    else
                        strToReturn = CombineDomainAndPath(p_strDomain, p_strUrl);
                } else { strToReturn = ""; }
                
            }
            else  //  The URL appears to be absolute, but might not have a scheme prefixed to it
            {
                // Exception: 
                //    * differing domain
                if (p_blnRemoveDifferingDomain)
                {
                    Uri uriNewDomain = new Uri(p_strUrl, UriKind.Absolute);
                    Uri uriIncomingDomain = new Uri(p_strDomain, UriKind.Absolute);
                    if (uriNewDomain.Host == uriIncomingDomain.Host)
                    {
                        strToReturn = "";
                    }
                }

                //  Prefix scheme if needed
                string strScheme = new Uri(p_strDomain, UriKind.Absolute).Scheme;
                if (!p_strUrl.StartsWith("http"))
                {
                    strToReturn = strScheme + "://" + p_strUrl;
                }
                else { strToReturn = p_strUrl; }    //  Return the URL as is since it's fully qualified
            }

            

            //  If all else fails, return with an empty string, otherwise with the well-formed string
            return strToReturn;

        }

        /// <summary>
        /// Combines a domain and path safely (taking duplicate slashes into consideration)
        /// </summary>
        /// <param name="p_strDomain">The domain to attach the path to</param>
        /// <param name="p_strUrl">The absolute path (either with slash at the beginning or without) that gets joined in</param>
        /// <returns></returns>
        public static string CombineDomainAndPath(string p_strDomain, string p_strUrl)
        {
            //  Check if the absolute path already contains the domain
            if (p_strUrl.StartsWith("http://") || p_strUrl.StartsWith("https://"))
                return p_strUrl;
            else if (p_strUrl.StartsWith(p_strDomain))
                return p_strDomain;
            else if (p_strUrl.StartsWith("http://" + p_strDomain))
                return "http://" + p_strUrl;
            else if (p_strUrl.StartsWith("https://" + p_strDomain))
                return "https://" + p_strUrl;

            if (!p_strDomain.Contains("://"))
                p_strDomain = "http://" + p_strDomain;

            //  Check leading and trailing slashes and combine the domain with path
            p_strDomain = (p_strDomain.EndsWith("/") && p_strUrl.StartsWith("/") ? p_strDomain.Substring(0, p_strDomain.Length - 1) : p_strDomain);
            return p_strDomain + (p_strDomain.EndsWith("/") || p_strUrl.StartsWith("/") ? "" : "/") + p_strUrl;
        }
        #endregion

    }
}
