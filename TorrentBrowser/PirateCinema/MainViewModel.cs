using System;
using System.Globalization;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using TorrentBrowser;

namespace PirateCinema
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        private IReadOnlyReactiveList<TorrentMovie> _moviesList;

        private string _selectedLangId;

        public MainViewModel()
        {
            Application.Current.Exit += Current_Exit;

            FetchCommand = ReactiveCommand.CreateAsyncObservable(_ => FetchMovies());
            SelectedLangId = SubtitleLanguage.FromCurrentCulture();

            InitializeCachedList();
        }

        public ICommand FetchCommand { get; set; }

        public IReadOnlyReactiveList<TorrentMovie> MoviesList
        {
            get { return _moviesList; }
            set { this.RaiseAndSetIfChanged(ref _moviesList, value); }
        }

        public string SelectedLangId
        {
            get { return _selectedLangId; }
            set { this.RaiseAndSetIfChanged(ref _selectedLangId, value); }
        }

        public IObservable<Unit> FetchMovies()
        {
            var movieList = MovieListFactory.Build();

            MoviesList = movieList.ByRatingWithSubtitle;

            return Observable.Start(() =>
            {
                var imdbWorker = new ImdbBrowserWorker(SubtitleLanguage.FromIsoName(SelectedLangId));
                var torrentWorker = new TorrentBrowserWorker();

                var moviesSource1 = torrentWorker.Work(TorrentSiteProvider.PirateBay, _cancelToken.Token);
                var moviesSource2 = torrentWorker.Work(TorrentSiteProvider.ExtraTorrent, _cancelToken.Token);
                var moviesSource3 = torrentWorker.Work(TorrentSiteProvider._1337x, _cancelToken.Token);

                var torrents = moviesSource1
                    .Merge(moviesSource2)
                    .Merge(moviesSource3);

                imdbWorker.Work(torrents, _cancelToken.Token)
                    .Distinct(x => x.Id)
                    .ObserveOn(RxApp.MainThreadScheduler)
                    .Subscribe(movieList.List.Add, _cancelToken.Token);
            });
        }

        private void InitializeCachedList()
        {
            var movies = MovieListFactory.Build(new ReactiveList<TorrentMovie>(new TorrentBrowserWorker().GetCache()));

            MoviesList = movies.ByRatingWithSubtitle;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            _cancelToken?.Cancel();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
                _cancelToken.Dispose();
            }

            GC.SuppressFinalize(this);
        }
    }
}
