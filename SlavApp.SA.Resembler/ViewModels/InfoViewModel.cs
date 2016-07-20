using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.SA.Resembler.ViewModels
{
    public class InfoViewModel : Screen
    {
        public InfoViewModel()
        {
            this.DisplayName = "Resembler";
        }
        public void Close()
        {
            TryClose(true);
        }
    }
}
