using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper.Configuration.Attributes;

namespace DAL.Models
{
    public class ChampionStats
    {
        [Ignore]
        public string Puuid { get; set; }
        public string Name { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public float WinRate { get => (float) Math.Round((Wins / (float) GamesPlayed), 2); set { } }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalAssists { get; set; }
        public int TotalMinionsKilled { get; set; }
        public float KDA { get => (float) Math.Round(((TotalKills + TotalAssists) / (float) Math.Max(1, TotalDeaths)), 2); set { } }
        public int TotalDamageDealtToTurrets { get; set; }
        public int DoubleKills { get; set; }
        public int TripleKills { get; set; }
        public int QuadraKills { get; set; }
        public int PentaKills { get; set; }
        public int TotalDamageDealtToChampions { get; set; }
        public int TotalDamageShieldedOnTeammates { get; set; }
        public int TotalHealsOnTeammates { get; set; }
        public int VisionScoreSum { get; set; }
        public int TotalVisionWardsBoughtInGame { get; set; }
        public int TotalWardsPlaced { get; set; }
        public string Grade { get; set; }
    }
}
