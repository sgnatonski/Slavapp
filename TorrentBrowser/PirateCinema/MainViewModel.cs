using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using TorrentBrowser;

namespace TorrentMoviesBrowser
{
    public class MainViewModel : ReactiveObject
    {
        public MainViewModel()
        {
            FetchCommand = ReactiveCommand.CreateAsyncObservable(x => FetchMovies());

            Movies = new ReactiveList<TorrentMovie>(new TorrentBrowserWorker().GetCache());
            MoviesByTitle = Movies.CreateDerivedCollection(
                x => x,
                x => true,
                (x, y) => x.Movie.CompareTo(y.Movie));

            MoviesByRating = Movies.CreateDerivedCollection(
                x => x,
                x => true,
                (x, y) => y.Rating.CompareTo(x.Rating));

            MoviesByRatingWithSubtitle = Movies.CreateDerivedCollection(
                x => x,
                x => x.HasSubtitles,
                (x, y) => y.Rating.CompareTo(x.Rating));

            MoviesByQuality = Movies.CreateDerivedCollection(
                x => x,
                x => true,
                (x, y) => y.Quality.CompareTo(x.Quality));

            MoviesList = MoviesByRatingWithSubtitle;
        }

        public ICommand FetchCommand { get; set; }
        
        private ReactiveList<TorrentMovie> Movies { get; set; }

        private IReactiveDerivedList<TorrentMovie> MoviesByTitle { get; set; }

        private IReactiveDerivedList<TorrentMovie> MoviesByRating { get; set; }

        private IReactiveDerivedList<TorrentMovie> MoviesByRatingWithSubtitle { get; set; }

        private IReactiveDerivedList<TorrentMovie> MoviesByQuality { get; set; }

        private IReadOnlyReactiveList<TorrentMovie> moviesList;
        public IReadOnlyReactiveList<TorrentMovie> MoviesList
        {
            get { return moviesList; }
            set { this.RaiseAndSetIfChanged(ref moviesList, value); }
        }

        public IObservable<Unit> FetchMovies()
        {
            Movies = new ReactiveList<TorrentMovie>();

            MoviesByTitle = Movies.CreateDerivedCollection(
                x => x,
                x => true,
                (x, y) => x.Movie.CompareTo(y.Movie));

            MoviesByRating = Movies.CreateDerivedCollection(
                x => x,
                x => true,
                (x, y) => y.Rating.CompareTo(x.Rating));

            MoviesByRatingWithSubtitle = Movies.CreateDerivedCollection(
                x => x,
                x => x.HasSubtitles,
                (x, y) => y.Rating.CompareTo(x.Rating));

            MoviesByQuality = Movies.CreateDerivedCollection(
                x => x,
                x => true,
                (x, y) => y.Quality.CompareTo(x.Quality));

            MoviesList = MoviesByRatingWithSubtitle;

            return Observable.Start(() =>
            {
                var movies1 = new TorrentBrowserWorker().Work(new TorrentSite
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

                var movies3 = new TorrentBrowserWorker().Work(new TorrentSite
                {
                    ListUrl = "http://extratorrent.cc/category/49/Thriller+Torrents.html",
                    PageBaseUrl = "http://extratorrent.cc",
                    ListItemSelector = "table.tl > tbody > tr > td.tli > a"
                }, new CancellationToken());

                //var movies = new FakeTorrentBrowserWorker().Work();

                var movies = movies1.Merge(movies3);

                movies.Subscribe(tm =>
                {
                    if (Application.Current != null)
                    {
                        Application.Current.Dispatcher.Invoke(() => Movies.Add(tm));
                    }                  
                });
            });
        }
    }    
}
