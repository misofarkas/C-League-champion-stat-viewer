using DAL.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class Repository
    {
        public async Task AddMatchAsync(Match match)
        {
            using (var db = new LoLDbContext())
            {
                await db.Matches.AddAsync(match);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateChampionStatsFromMatches(string summonerName, string serverName, string APIKey)
        {
            var puuid = await APIMethods.GetSummonerPuuidAsync(summonerName, serverName, APIKey);
            var matches = await GetAllMatchesAsync(puuid);
            await RemoveChampionStatsAsync(puuid);

            foreach (Match match in matches)
            {
                await AddOrUpdateChampionStats(match);
            }
        }
        
        private async Task AddOrUpdateChampionStats(Match match)
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
        public async Task RemoveChampionStatsAsync(string puuid)
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
        public async Task<List<ChampionStats>> GetAllChampionStatsAsync(string puuid)
        {
            using (var db = new LoLDbContext())
            {
                return await db.ChampionStats.Where(x => x.Puuid == puuid).ToListAsync();
            }
        }
        
        
        public async Task<List<Match>> GetAllMatchesAsync(string puuid)
        {

            using (var db = new LoLDbContext())
            {
                return await db.Matches.Where(x => x.Puuid == puuid).ToListAsync();
            }
        }
        
        public async Task LoadMatchesAsync(string summonerName, string serverName, string region, string APIKey, int count = 5)
        {
            var puuid = await APIMethods.GetSummonerPuuidAsync(summonerName, serverName, APIKey);
            var matchIDs = await APIMethods.GetSummonerMatchIDsAsync(region, puuid, APIKey);

            var index = 0;
            foreach (var id in matchIDs)
            {
                if (index >= count)
                    break;

                if (index % 100 == 90)
                    await Task.Delay(121000);
                
                try
                {
                    await AddMatchAsync(await APIMethods.GetMatchStatsAsync(region, puuid, APIKey, id));
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
