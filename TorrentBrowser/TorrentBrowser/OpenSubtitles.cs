using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace TorrentBrowser
{
    public static class OpenSubtitles
    {
        public static string[] GetSubtitles(int imdbid, string langid)
        {
            try
            {
                var osXml = XDocument.Load($"http://www.opensubtitles.org/en/search/imdbid-{imdbid}/sublanguageid-{langid}/xml", LoadOptions.None);
                return osXml.XPathSelectElements("//opensubtitles/search/results/subtitle/IDSubtitle").Select(x => x.Attribute("LinkDownload").Value).ToArray();
            }
            catch (Exception)
            {
                // ignored
            }

            return new string[0];
        }
    }
}
