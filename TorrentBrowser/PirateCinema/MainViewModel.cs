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
            FetchCommand = ReactiveCommand.CreateAsyncObservable(x => FetchMovies());

            var movies = MovieListFactory.Build(new ReactiveList<TorrentMovie>(new TorrentBrowserWorker().GetCache()));

            MoviesList = movies.ByRatingWithSubtitle;
        }

        public ICommand FetchCommand { get; set; }

        private IReadOnlyReactiveList<TorrentMovie> moviesList;
        public IReadOnlyReactiveList<TorrentMovie> MoviesList
        {
            get { return moviesList; }
            set { this.RaiseAndSetIfChanged(ref moviesList, value); }
        }

        public IObservable<Unit> FetchMovies()
        {
            var movieList = MovieListFactory.Build();
            MoviesList = movieList.ByRatingWithSubtitle;

            return Observable.Start(() =>
            {
                var moviesSource1 = new TorrentBrowserWorker().Work(new TorrentSite
                {
                    ListUrl = "https://thepiratebay.org/top/201",
                    PageBaseUrl = "https://thepiratebay.org/",
                    ListItemSelector = "#searchResult > tbody > tr > td > div > a"
                }, new CancellationToken());

                //var movies = new TorrentBrowserWorker().Work(new TorrentSite
                //{
                //    ListUrl = "https://kat.cr/movies/?field=seeders&sorder=desc",
                //    PageBaseUrl = "https://kat.cr/",
                //    ListItemSelector = "table > tbody > tr > td > div.torrentname > div > a"
                //}, new CancellationToken());

                var moviesSource3 = new TorrentBrowserWorker().Work(new TorrentSite
                {
                    ListUrl = "http://extratorrent.cc/category/49/Thriller+Torrents.html",
                    PageBaseUrl = "http://extratorrent.cc",
                    ListItemSelector = "table.tl > tbody > tr > td.tli > a"
                }, new CancellationToken());

                //var movies = new FakeTorrentBrowserWorker().Work();

                var moviesMerged = moviesSource1.Merge(moviesSource3);

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
