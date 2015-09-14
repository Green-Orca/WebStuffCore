using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;

using HtmlAgilityPack;

using WebStuff.Models;

namespace WebStuff.Services
{
    public class Sniffer
    {
        public Page Sniff(string p_strURL, string p_strMethod = "GET")
        {
            //  TODO: check URL well-formedness

            Page pCurrentPage = new Page();
            pCurrentPage.URL = p_strURL;
            
            try {
                //  We've got a working URL, let's get on with grabbing more page info
                //  Build up the request to the URL and grab the stream from the response (i.e. the source of the page)
                IPEndPoint iepRemoteEP = null;
                HttpWebRequest hwrqRequest = (HttpWebRequest)WebRequest.Create(new Uri(p_strURL));
                hwrqRequest.AllowAutoRedirect = false;  //  Switch off AutoRedirect so that we can capture 301s & 302s

                hwrqRequest.ServicePoint.BindIPEndPointDelegate = delegate(ServicePoint p_spServicePoint, IPEndPoint p_iepRemoteEP, int p_intRetryCount)
                {
                    iepRemoteEP = p_iepRemoteEP;
                    return null;
                };
                hwrqRequest.Method = p_strMethod.ToUpper();

                WebResponse wrpResponse = null;
                
                //  Getting a 404 throws a WebException so handle that here.
                try
                {
                    wrpResponse = hwrqRequest.GetResponse();
                }
                catch (WebException wex)
                {
                    wrpResponse = (WebResponse)wex.Response;
                    pCurrentPage.StatusCode = HttpStatusCode.NotFound;
                    pCurrentPage.LastError = wex.ToString();
                }
                catch (Exception ex)
                {
                    pCurrentPage.LastError = ex.ToString();
                }

                //  If we've got a response, continue grabbing details
                if (wrpResponse != null)
                {

                    HttpWebResponse hwrpResponse = (HttpWebResponse)wrpResponse;
                    //HttpWebRequest hwrqRequest = (HttpWebRequest)wrqRequest;

                    //  Add status code to page properties
                    pCurrentPage.StatusCode = hwrpResponse.StatusCode;

                    //  Grab page info from HTML page contents
                    using (Stream stmPageStream = wrpResponse.GetResponseStream())
                    {
                        using (StreamReader srdrPageReader = new StreamReader(stmPageStream))
                        {
                            HtmlDocument htmlDocDocument = new HtmlDocument();
                            htmlDocDocument.Load(srdrPageReader);
                            pCurrentPage.RequestHeader = "<strong>----------- By Keys -----------</strong><br /> ";
                            pCurrentPage.RequestHeader += hwrqRequest.Headers.AllKeys.OrderBy(key => key).Aggregate(string.Empty, (curr, _new) => curr + "<strong>" + _new + ":</strong> " + hwrqRequest.Headers[_new] + "<br />");
                            pCurrentPage.RequestHeader += "<strong>----------- By Properties -----------</strong><br /> ";
                            pCurrentPage.RequestHeader += "<strong>Accept:</strong> " + hwrqRequest.Accept + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Address:</strong> " + hwrqRequest.Address + "<br />";
                            pCurrentPage.RequestHeader += "<strong>AllowAutoRedirect:</strong> " + hwrqRequest.AllowAutoRedirect + "<br />";
                            pCurrentPage.RequestHeader += "<strong>AllowWriteStreamBuffering:</strong> " + hwrqRequest.AllowWriteStreamBuffering + "<br />";
                            pCurrentPage.RequestHeader += "<strong>AutomaticDecompression:</strong> " + hwrqRequest.AutomaticDecompression + "<br />";
                            pCurrentPage.RequestHeader += "<strong>CachePolicy:</strong> " + hwrqRequest.CachePolicy + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Connection:</strong> " + hwrqRequest.Connection + "<br />";
                            pCurrentPage.RequestHeader += "<strong>ContentLength:</strong> " + hwrqRequest.ContentLength + "<br />";
                            pCurrentPage.RequestHeader += "<strong>ContentType:</strong> " + hwrqRequest.ContentType + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Date:</strong> " + hwrqRequest.Date + "<br />";
                            pCurrentPage.RequestHeader += "<strong>HaveResponse:</strong> " + hwrqRequest.HaveResponse + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Host:</strong> " + hwrqRequest.Host + "<br />";
                            pCurrentPage.RequestHeader += "<strong>IfModifiedSince:</strong> " + hwrqRequest.IfModifiedSince + "<br />";
                            pCurrentPage.RequestHeader += "<strong>KeepAlive:</strong> " + hwrqRequest.KeepAlive + "<br />";
                            pCurrentPage.RequestHeader += "<strong>MaximumAutomaticRedirections:</strong> " + hwrqRequest.MaximumAutomaticRedirections + "<br />";
                            pCurrentPage.RequestHeader += "<strong>MaximumResponseHeadersLength:</strong> " + hwrqRequest.MaximumResponseHeadersLength + "<br />";
                            pCurrentPage.RequestHeader += "<strong>MediaType:</strong> " + hwrqRequest.MediaType + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Method:</strong> " + hwrqRequest.Method + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Pipelined:</strong> " + hwrqRequest.Pipelined + "<br />";
                            pCurrentPage.RequestHeader += "<strong>PreAuthenticate:</strong> " + hwrqRequest.PreAuthenticate + "<br />";
                            pCurrentPage.RequestHeader += "<strong>ProtocolVersion:</strong> " + hwrqRequest.ProtocolVersion + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Proxy:</strong> " + hwrqRequest.Proxy.ToString() + "<br />";
                            pCurrentPage.RequestHeader += "<strong>ReadWriteTimeout:</strong> " + hwrqRequest.ReadWriteTimeout + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Referer:</strong> " + hwrqRequest.Referer + "<br />";
                            pCurrentPage.RequestHeader += "<strong>RequestUri:</strong> " + hwrqRequest.RequestUri + "<br />";
                            pCurrentPage.RequestHeader += "<strong>SendChunked:</strong> " + hwrqRequest.SendChunked + "<br />";
                            pCurrentPage.RequestHeader += "<strong>ServicePoint Address:</strong> " + hwrqRequest.ServicePoint.Address + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Timeout:</strong> " + hwrqRequest.Timeout + "<br />";
                            pCurrentPage.RequestHeader += "<strong>TransferEncoding:</strong> " + hwrqRequest.TransferEncoding + "<br />";
                            pCurrentPage.RequestHeader += "<strong>UnsafeAuthenticatedConnectionSharing:</strong> " + hwrqRequest.UnsafeAuthenticatedConnectionSharing + "<br />";
                            pCurrentPage.RequestHeader += "<strong>UseDefaultCredentials:</strong> " + hwrqRequest.UseDefaultCredentials + "<br />";
                            pCurrentPage.RequestHeader += "<strong>UserAgent:</strong> " + hwrqRequest.UserAgent + "<br />";
                            pCurrentPage.RequestHeader += "<strong>Remote Address:</strong> " + hwrqRequest.Headers["REMOTE_ADDR"] + "<br />";

                            pCurrentPage.ResponseHeader = "<strong>----------- By Keys -----------</strong><br /> ";
                            pCurrentPage.ResponseHeader += hwrpResponse.Headers.AllKeys.OrderBy(key => key).Aggregate(string.Empty, (curr, _new) => curr + "<strong>" + _new + ":</strong> " + hwrpResponse.Headers[_new] + "<br />");
                            pCurrentPage.ResponseHeader += "<strong>----------- By Properties -----------</strong><br /> ";
                            pCurrentPage.ResponseHeader += "<strong>CharacterSet:</strong> " + hwrpResponse.CharacterSet + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>ContentEncoding:</strong> " + hwrpResponse.ContentEncoding + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>ContentLength:</strong> " + hwrpResponse.ContentLength + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>ContentType:</strong> " + hwrpResponse.ContentType + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>IsFromCache:</strong> " + hwrpResponse.IsFromCache + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>IsMutuallyAuthenticated:</strong> " + hwrpResponse.IsMutuallyAuthenticated + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>LastModified:</strong> " + hwrpResponse.LastModified + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>Method:</strong> " + hwrpResponse.Method + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>ProtocolVersion:</strong> " + hwrpResponse.ProtocolVersion + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>ResponseUri:</strong> " + hwrpResponse.ResponseUri + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>Server:</strong> " + hwrpResponse.Server + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>StatusCode:</strong> " + hwrpResponse.StatusCode + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>StatusDescription:</strong> " + hwrpResponse.StatusDescription + "<br />";
                            pCurrentPage.ResponseHeader += "<strong>Remote Address:</strong> " + hwrpResponse.Headers["REMOTE_ADDR"] + "<br />";

                            pCurrentPage.RemoteIP = iepRemoteEP.Address + "";

                        }
                    }
                    
                    //  Close response objects
                    hwrpResponse.Close();
                    wrpResponse.Close();
                }
            }
            catch (Exception ex)
            {
                pCurrentPage.LastError = ex.Message;
            }


            return pCurrentPage;

        }

    }
}
