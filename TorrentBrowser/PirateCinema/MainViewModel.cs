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
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            Application.Current.Exit += Current_Exit;

            FetchCommand = ReactiveCommand.CreateAsyncObservable(_ => FetchMovies());

            var movies = MovieListFactory.Build(new ReactiveList<TorrentMovie>(new TorrentBrowserWorker().GetCache()));

            MoviesList = movies.ByRatingWithSubtitle;
        }

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            _cancelToken.Cancel();
        }

        public ICommand FetchCommand { get; set; }

        private IReadOnlyReactiveList<TorrentMovie> _moviesList;
        public IReadOnlyReactiveList<TorrentMovie> MoviesList
        {
            get { return _moviesList; }
            set { this.RaiseAndSetIfChanged(ref _moviesList, value); }
        }

        private CancellationTokenSource _cancelToken = new CancellationTokenSource();

        public IObservable<Unit> FetchMovies()
        {
            _cancelToken = new CancellationTokenSource();

            var movieList = MovieListFactory.Build();

            MoviesList = movieList.ByRatingWithSubtitle;

            return Observable.Start(() =>
            {
                var worker = new TorrentBrowserWorker();

                var moviesSource1 = worker.Work(TorrentSiteProvider.PirateBay, SubtitleLanguage.Polish, _cancelToken.Token);
                var moviesSource2 = worker.Work(TorrentSiteProvider.ExtraTorrent, SubtitleLanguage.Polish, _cancelToken.Token);
                //var moviesSource3 = worker.Work(TorrentSiteProvider.Kat, SubtitleLanguage.Polish, cancelToken.Token);

                var moviesMerged = moviesSource1.Merge(moviesSource2);

                moviesMerged.Subscribe(tm =>
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => movieList.List.Add(tm));
                    }                  
                });
            });
        }
    }    
}
