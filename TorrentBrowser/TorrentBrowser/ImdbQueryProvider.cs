using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TorrentBrowser
{
    public class ImdbQueryProvider
    {
        public string OriginalTitleQuery => "#title-overview-widget > div.vital > div.title_block > div > div.titleBar > div.title_wrapper > div.originalTitle";
        public string TitleQuery => "#title-overview-widget > div.vital > div.title_block > div > div.titleBar > div.title_wrapper > h1";
        public string RatingQuery => "#title-overview-widget > div.vital > div.title_block > div > div.ratings_wrapper > div.imdbRating > div.ratingValue > strong > span";
        public string PictureQuery => "#title-overview-widget > div.vital > div.slate_wrapper > div.poster > a > img";
    }
}
