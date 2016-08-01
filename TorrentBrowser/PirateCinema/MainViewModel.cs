using System;
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

        public MainViewModel()
        {
            Application.Current.Exit += Current_Exit;

            FetchCommand = ReactiveCommand.CreateAsyncObservable(_ => FetchMovies());

            InitializeCachedList();
        }

        public ICommand FetchCommand { get; set; }

        public IReadOnlyReactiveList<TorrentMovie> MoviesList
        {
            get { return _moviesList; }
            set { this.RaiseAndSetIfChanged(ref _moviesList, value); }
        }

        public IObservable<Unit> FetchMovies()
        {
            var movieList = MovieListFactory.Build();

            MoviesList = movieList.ByRatingWithSubtitle;

            return Observable.Start(() =>
            {
                var worker = new TorrentBrowserWorker();

                var moviesSource1 = worker.Work(TorrentSiteProvider.PirateBay, SubtitleLanguage.Polish, _cancelToken.Token);
                var moviesSource2 = worker.Work(TorrentSiteProvider.ExtraTorrent, SubtitleLanguage.Polish, _cancelToken.Token);
                var moviesSource3 = worker.Work(TorrentSiteProvider._1337x, SubtitleLanguage.Polish, _cancelToken.Token);

                var moviesMerged = moviesSource1.Merge(moviesSource2).Merge(moviesSource3);

                moviesMerged.Subscribe(tm => Application.Current.Dispatcher.Invoke(() => movieList.List.Add(tm)), _cancelToken.Token);
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
