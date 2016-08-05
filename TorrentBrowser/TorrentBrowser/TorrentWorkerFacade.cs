using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;

namespace TorrentBrowser
{
    public class TorrentWorkerFacade
    {
        private readonly CancellationToken _cancellationToken;
        private readonly TorrentBrowserWorker _torrentWorker;
        private readonly ImdbBrowserWorker _imdbWorker;

        public TorrentWorkerFacade(SubtitleLanguage subtitleLang, CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            _imdbWorker = new ImdbBrowserWorker(subtitleLang);
            _torrentWorker = new TorrentBrowserWorker();
        }

        public IObservable<TorrentMovie> Work(params TorrentSite[] sites)
        {
            var torrents = sites.Select(site => _torrentWorker.Work(site, _cancellationToken))
                .Aggregate((source1, source2) => source1.Merge(source2));

            var imdbTorrents = _imdbWorker.Work(torrents, _cancellationToken);

            return _torrentWorker.UpdateCache(imdbTorrents)
                .Select(x => x.TorrentMovie)
                .Distinct(x => x.Id);
        }
    }
}
