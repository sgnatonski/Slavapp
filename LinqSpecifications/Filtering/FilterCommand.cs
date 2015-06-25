using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqSpecifications.Filtering
{
    public sealed class FilterCommand
    {
        public string Column { get; set; }

        public string Filter { get; set; }

        public bool Independent { get; set; }

        public FilterOperation Operation { get; set; }
    }
}
