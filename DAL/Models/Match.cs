using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class Match
    {
        public string MatchID { get; set; }
        public string Puuid { get; set; }
        public string ChampionName { get; set; }
        public int ChampionID { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int TotalMinionsKilled { get; set; }
        public int DamageDealtToTurrets { get; set; }
        public int QuadraKills { get; set; }
        public int PentaKills { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageShieldedOnTeammates { get; set; }
        public int TotalHealsOnTeammates { get; set; }
        public int VisionScore { get; set; }
        public int VisionWardsBoughtInGame { get; set; }
        public int WardsPlaced { get; set; }
        public bool Win { get; set; }
    }
}
