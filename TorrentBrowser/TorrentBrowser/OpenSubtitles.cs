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
        public static async Task<SubtitleData[]> GetSubtitles(int imdbid, SubtitleLanguage langid)
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
                    var subs = osXml.XPathSelectElements("//opensubtitles/search/results/subtitle").Select(x => new SubtitleData
                    {
                        LinkDownload = x.Element("IDSubtitle")?.Attribute("LinkDownload")?.Value,
                        ReleaseName = x.Element("MovieReleaseName")?.Value
                    }).ToArray();
                    return subs;
                }
            }
            catch (Exception ex)
            {
                // ignored
            }

            return new SubtitleData[0];
        }
    }
}
