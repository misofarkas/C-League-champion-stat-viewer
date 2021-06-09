using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Player
    {
        public string Puuid { get; set; }
        public long LatestMatchID { get; set; }
        public long OldestMatchID { get; set; }
    }
}
