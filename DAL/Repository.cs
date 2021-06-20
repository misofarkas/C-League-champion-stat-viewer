using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class Repository
    {
        // Adds a single match into the DB
        public static async Task AddMatchAsync(Match match)
        {
            using (var db = new LoLDbContext())
            {
                await db.Matches.AddAsync(match);
                await db.SaveChangesAsync();
            }
        }

        // Removes and updates all champion stats from matches saved in the DB for a given player
        public static async Task UpdateChampionStatsFromMatches(string summonerName, string serverName)
        {
            var puuid = await APIMethods.GetSummonerPuuidAsync(summonerName, serverName);
            var matches = await GetAllMatchesAsync(puuid);
            await RemoveChampionStatsAsync(puuid);

            foreach (Match match in matches)
            {
                await AddOrUpdateChampionStats(match);
            }
        }

        // If a champion does not exist in the DB for a given player,
        // Create, update it's stats and add it to the DB
        private static async Task AddOrUpdateChampionStats(Match match)
        {
            
            using (var db = new LoLDbContext())
            {
                var champion = db.ChampionStats.SingleOrDefault(x => x.Name == match.ChampionName && x.Puuid == match.Puuid);
                if (champion == null)
                {
                    champion = new ChampionStats
                    {
                        Name = match.ChampionName,
                        Puuid = match.Puuid,
                        Grade = "-"
                    };
                    await db.ChampionStats.AddAsync(champion);
                }

                champion.GamesPlayed++;
                champion.Wins = match.Win ? champion.Wins + 1 : champion.Wins;
                champion.TotalKills += match.Kills;
                champion.TotalDeaths += match.Deaths;
                champion.TotalAssists += match.Assists;
                champion.TotalMinionsKilled += match.TotalMinionsKilled;
                champion.TotalDamageDealtToTurrets += match.DamageDealtToTurrets;
                champion.DoubleKills += match.DoubleKills;
                champion.TripleKills += match.TripleKills;
                champion.QuadraKills += match.QuadraKills;
                champion.PentaKills += match.PentaKills;
                champion.TotalDamageDealtToChampions += match.TotalDamageDealtToChampions;
                champion.TotalDamageShieldedOnTeammates += match.TotalDamageShieldedOnTeammates;
                champion.TotalHealsOnTeammates += match.TotalHealsOnTeammates;
                champion.VisionScoreSum += match.VisionScore;
                champion.TotalVisionWardsBoughtInGame += match.VisionWardsBoughtInGame;
                champion.TotalWardsPlaced += match.WardsPlaced;

                await db.SaveChangesAsync();

            }
        }

        // Removes a champion record from the DB for a given player
        public static async Task RemoveChampionStatsAsync(string puuid)
        {
            var playerChampionStats = await GetAllChampionStatsAsync(puuid);
            using (var db = new LoLDbContext())
            {
                foreach(var champion in playerChampionStats)
                {
                    db.ChampionStats.Remove(champion);
                }
                await db.SaveChangesAsync();
            }
        }

        // Returns a list of all champions corresponding to a player's puuid
        public static async Task<List<ChampionStats>> GetAllChampionStatsAsync(string puuid)
        {
            using (var db = new LoLDbContext())
            {
                return await db.ChampionStats.Where(x => x.Puuid == puuid).ToListAsync();
            }
        }

        // Returns a list of all matches corresponding to a player's puuid
        public static async Task<List<Match>> GetAllMatchesAsync(string puuid)
        {

            using (var db = new LoLDbContext())
            {
                return await db.Matches.Where(x => x.Puuid == puuid).ToListAsync();
            }
        }

        // Loads up to *count* last matches played for a given player
        // and adds them to the DB
        public static async Task LoadMatchesAsync(string summonerName, string serverName, string region, int count)
        {
            var puuid = await APIMethods.GetSummonerPuuidAsync(summonerName, serverName);
            var matchIDs = await APIMethods.GetSummonerMatchIDsAsync(region, puuid);

            var index = 0;
            foreach (var id in matchIDs)
            {
                if (index >= count)
                    break;

                // In order not to exceed the API's rate-limit, there has to be a 2 minute delay
                // after every 90 matches loaded
                if (index % 100 == 90)
                    await Task.Delay(121000);
                
                try
                {
                    await AddMatchAsync(await APIMethods.GetMatchStatsAsync(region, puuid, id));
                }
                catch (DbUpdateException)
                {
                    // Catch duplicate matches
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    // Catch 404 match not found exception
                }
                
                index++;
            }

        }
    }
}
