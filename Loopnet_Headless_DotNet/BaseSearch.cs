using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loopnet_Headless_DotNet
{
    class BaseSearch
    {
        public string Name { get; set; }
        public string BaseResultsURL { get; set; }
        public List<Listing> Listings = new List<Listing>();
    }

    class Listing
    {
        public string PropertyName { get; set; }
        public string BldgClass { get; set; }
    }
}
