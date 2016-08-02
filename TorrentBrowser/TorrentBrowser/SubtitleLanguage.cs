using System.Threading;

namespace TorrentBrowser
{
    public class SubtitleLanguage
    {
        private readonly string _langid;

        public static SubtitleLanguage Polish = new SubtitleLanguage("pol");

        public SubtitleLanguage(string langid)
        {
            _langid = langid;
        }

        public static SubtitleLanguage FromIsoName(string isoName)
        {
            return new SubtitleLanguage(isoName);
        }

        public static SubtitleLanguage FromCurrentCulture()
        {
            return new SubtitleLanguage(Thread.CurrentThread.CurrentCulture.ThreeLetterISOLanguageName);
        }

        public static implicit operator string(SubtitleLanguage l)
        {
            return l.ToString();
        }

        public override string ToString()
        {
            return _langid;
        }
    }
}
