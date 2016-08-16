using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TorrentBrowser
{
    public static class OpenSubtitles
    {
        public static async Task<string[]> GetSubtitles(int imdbid, SubtitleLanguage langid)
        {
            try
            {
                Debug.WriteLine($"Requesting http://www.opensubtitles.org/en/search/imdbid-{imdbid}/sublanguageid-{langid}/xml");
                var rq = (HttpWebRequest)WebRequest.Create($"http://www.opensubtitles.org/en/search/imdbid-{imdbid}/sublanguageid-{langid}/xml");

                rq.Timeout = 1000;
                rq.ReadWriteTimeout = 10000;

                var response = await rq.GetResponseAsync() as HttpWebResponse;

                using (var responseStream = response.GetResponseStream())
                {
                    XmlTextReader reader = new XmlTextReader(responseStream);
                    var osXml = XDocument.Load(reader, LoadOptions.None);
                    return osXml.XPathSelectElements("//opensubtitles/search/results/subtitle/IDSubtitle").Select(x => x.Attribute("LinkDownload").Value).ToArray();
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            return new string[0];
        }
    }
}
