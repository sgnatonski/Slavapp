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
