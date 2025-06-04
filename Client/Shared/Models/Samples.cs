using System.Collections.Generic;
using System.Linq;

namespace JLioOnline.Client.Shared.Models
{
    public class Samples : List<Sample>
    {
        public Samples()
        {
        }

        public Samples(Sample[] result) : base(result.ToList())
        {
        }
    }
}