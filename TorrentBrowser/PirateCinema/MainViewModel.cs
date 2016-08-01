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
            FetchCommand = ReactiveCommand.CreateAsyncObservable(_ => FetchMovies());

            var movies = MovieListFactory.Build(new ReactiveList<TorrentMovie>(new TorrentBrowserWorker().GetCache()));

            MoviesList = movies.ByRatingWithSubtitle;
        }

        public ICommand FetchCommand { get; set; }

        private IReadOnlyReactiveList<TorrentMovie> _moviesList;
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
                var cancelToken = new CancellationTokenSource();

                var worker = new TorrentBrowserWorker();

                var moviesSource1 = worker.Work(TorrentSiteProvider.PirateBay, SubtitleLanguage.Polish, cancelToken.Token);
                var moviesSource2 = worker.Work(TorrentSiteProvider.ExtraTorrent, SubtitleLanguage.Polish, cancelToken.Token);
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
