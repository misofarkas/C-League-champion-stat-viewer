using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Models
{
    public class ChampionStats
    {
        public string Puuid { get; set; }
        public string Name { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public float WinRate { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalAssists { get; set; }
        public int TotalMinionsKilled { get; set; }
        public float KDA { get; set; }
        public int TotalDamageDealtToTurrets { get; set; }
        public int QuadraKills { get; set; }
        public int PentaKills { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageShieldedOnTeammates { get; set; }
        public int TotalHealsOnTeammates { get; set; }
        public int VisionScoreSum { get; set; }
        public int TotalVisionWardsBoughtInGame { get; set; }
        public int TotalWardsPlaced { get; set; }
    }
}
