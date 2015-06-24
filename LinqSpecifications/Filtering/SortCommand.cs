using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqSpecifications.Filtering
{
    public class SortCommand
    {
        public string Column { get; set; }

        public bool Ascending { get; set; }
    }
}
