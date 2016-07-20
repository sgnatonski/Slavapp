using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.Minion.Resembler.ViewModels
{
    public class DriveSelectionModel : PropertyChangedBase
    {
        private string drive;
        public string Drive
        {
            get { return drive; }
            set
            {
                drive = value;
                NotifyOfPropertyChange(() => Drive);
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }
    }
}
