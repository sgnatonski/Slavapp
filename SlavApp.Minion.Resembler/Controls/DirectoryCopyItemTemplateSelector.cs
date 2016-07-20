using SlavApp.Minion.Resembler.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SlavApp.Minion.Resembler.Controls
{
    public class DirectoryCopyItemTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var model = item as DirectoryCopyResultModel;
            var resourceName = String.Empty;
            if (model.ExactCopy)
            {
                resourceName = "ExactCopyItem";
            }
            else if (model.Dir1ContainsDir2)
            {
                resourceName = "Dir1ContainsDir2Item";
            }
            else if (model.Dir2ContainsDir1)
            {
                resourceName = "Dir2ContainsDir1Item";
            }
            else if (model.SubsetCopy)
            {
                resourceName = "SubsetCopyItem";
            }
            var element = container as FrameworkElement;
            return element.FindResource(resourceName) as DataTemplate;
        }
    }
}
