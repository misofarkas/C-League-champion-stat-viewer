using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public static class Grades
    {
        // Base line stats for calculating champion's grade
        private static Dictionary<string, double> baselineStats = new Dictionary<string, double> 
        {
            {"winrate", 0.5 },
            {"kda", 2.5 },
            {"minionsKilled", 150 },
            {"turretDamage", 2500 },
            {"championDamage", 18000 },
            {"shields", 2000 },
            {"heals", 2000 },
            {"visionScore", 20 },
            {"visionWardsBought", 2 },
            {"visionScoreSupp", 50 },
            {"visionWardsBoughtSupp", 6 }
        };

        // Updates grades for all champions for a given player
        public static async Task UpdateGrades(string puuid)
        {

            using (var db = new LoLDbContext())
            {
                var champions = db.ChampionStats.Where(x => x.Puuid == puuid);

                foreach (ChampionStats champion in champions)
                {
                    champion.Grade = CalculateGrade(champion);
                }

                await db.SaveChangesAsync();
            }
        } 

        // Calculates a score from champion stats, comparing it to base line stats
        // and evaluates the grade
        private static string CalculateGrade(ChampionStats champion)
        {
            // Skip calculating grade for a champion with less than 3 games played
            if (champion.GamesPlayed < 3)
                return "-";
            
            double score = 0;

            score += (champion.WinRate - baselineStats["winrate"]) * 150;
            score += (champion.KDA - baselineStats["kda"]) * 7;

            // assume champion is not a support based on average minions killed
            if (Average(champion.TotalMinionsKilled, champion.GamesPlayed) > 40)
            {
                score += (Average(champion.TotalMinionsKilled, champion.GamesPlayed) - baselineStats["minionsKilled"]) * 0.25;
                score += (Average(champion.TotalDamageDealtToTurrets, champion.GamesPlayed) - baselineStats["turretDamage"]) / 250;
                score += (Average(champion.TotalDamageDealtToChampions, champion.GamesPlayed) - baselineStats["championDamage"]) / 200;

                score += (Average(champion.VisionScoreSum, champion.GamesPlayed) - baselineStats["visionScore"]) / 3;
                score += (Average(champion.TotalVisionWardsBoughtInGame, champion.GamesPlayed) - baselineStats["visionWardsBought"]) * 3;
            }
            // assume champion is a support
            else
            {
                score += (Average(champion.TotalDamageShieldedOnTeammates, champion.GamesPlayed) - baselineStats["shields"]) / 1000;
                score += (Average(champion.TotalHealsOnTeammates, champion.GamesPlayed) - baselineStats["heals"]) / 1000;

                score += (Average(champion.VisionScoreSum, champion.GamesPlayed) - baselineStats["visionScoreSupp"]);
                score += (Average(champion.TotalVisionWardsBoughtInGame, champion.GamesPlayed) - baselineStats["visionWardsBoughtSupp"]) * 3;
            }


            if (score > 100)
                return "S+";
            if (score > 75)
                return "S";
            if (score > 50)
                return "A";
            if (score > 25)
                return "B";
            if (score > 0)
                return "C";
            return "D";
            
            
        }

        private static double Average(double stat, int gamesPlayed)
        {
            return Math.Round((stat / (double)gamesPlayed), 2);
        }
    }
}
