using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public static class ServerDict
    {
        public static Dictionary<string, string> ServerDictionary = new Dictionary<string, string>{
            {"EUN1", "EUROPE"},
            {"EUW1", "EUROPE"},
            {"RU", "EUROPE"},
            {"TR1", "EUROPE"},
            {"NA1", "AMERICAS"},
            {"LA1", "AMERICAS"},
            {"LA2", "AMERICAS"},
            {"BR1", "AMERICAS"},
            {"KR", "ASIA"},
            {"JP1", "ASIA"},
            {"OC1", "ASIA"}
        };
    }
}
