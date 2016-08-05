using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using TorrentBrowser;

namespace PirateCinema
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly CancellationTokenSource _cancelToken = new CancellationTokenSource();

        public MainViewModel()
        {
            Application.Current.Exit += Current_Exit;

            FetchCommand = ReactiveCommand.CreateAsyncObservable(_ => FetchMovies());
            SelectedLangId = SubtitleLanguage.FromCurrentCulture();

            InitializeCachedList();
        }

        public ICommand FetchCommand { get; set; }

        [Reactive]
        public IReadOnlyReactiveList<TorrentMovie> MoviesList { get; set; }

        [Reactive]
        public string SelectedLangId { get; set; }

        public IObservable<Unit> FetchMovies()
        {
            var movieList = MovieListFactory.Build();

            MoviesList = movieList.ByRatingWithSubtitle;

            return Observable.Start(() =>
            {
                var torrentWorker = new TorrentWorkerFacade(SubtitleLanguage.FromIsoName(SelectedLangId), _cancelToken.Token);

                torrentWorker.Work(
                    TorrentSiteProvider.PirateBay, 
                    TorrentSiteProvider.ExtraTorrent, 
                    TorrentSiteProvider._1337x)
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
