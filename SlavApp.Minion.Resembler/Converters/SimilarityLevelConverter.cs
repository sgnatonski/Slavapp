using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace SlavApp.Minion.Resembler.Converters
{
    public class SimilarityLevelConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var val = System.Convert.ToInt32(value);
            switch (val)
            {
                case 0:
                case 1:
                case 2:
                    return "Identical";
                case 3:
                case 4:
                case 5:
                    return "Almost identical";
                case 6:
                case 7:
                case 8:
                    return "Very similar";
                case 9:
                case 10:
                case 11:
                case 12:
                    return "Similar";
                case 13:
                case 14:
                case 15:
                    return "Related";
                case 16:
                case 17:
                case 18:
                    return "Somewhat related";
                case 19:
                case 20:
                    return "Almost unrelated";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
