using DAL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class APIMethods
    {

        public static async Task<string> GetAsync(string url)
        {
            using var client = new HttpClient();
            var content = await client.GetStringAsync(url);
            return content;
        }
        public static async Task<string> GetSummonerPuuidAsync(string summonerName, string serverName, string APIKey)
        {
            var uri = $"https://{serverName}.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}?api_key={APIKey}";
            var result = await GetAsync(uri);
            dynamic jsonResult = JsonConvert.DeserializeObject(result);
            return jsonResult["puuid"];
        }

        public static async Task<List<string>> GetSummonerMatchIDsAsync(string region, string  puuid, string APIKey)
        {
            var matchIds = new List<string>();
            
            var index = 0;
            bool stop = false;
            while (stop == false)
            {
                var uri = $"https://{region}.api.riotgames.com/lol/match/v5/matches/by-puuid/{puuid}/ids?start={index}&count=100&api_key={APIKey}";
                var result = await GetAsync(uri);
                dynamic jsonResult = JsonConvert.DeserializeObject(result);

                foreach (var id in jsonResult)
                {
                    matchIds.Add((string)id);
                }

                index += 100;

                if (jsonResult.Count < 100)
                    stop = true;

            } 

            return matchIds;
        }

        public static async Task<Match> GetMatchStatsAsync(string region, string puuid, string APIKey, string matchID)
        {
            var uri = $"https://{region}.api.riotgames.com/lol/match/v5/matches/{matchID}?api_key={APIKey}";

            var result = await GetAsync(uri);
            dynamic jsonResult = JsonConvert.DeserializeObject(result);
            var match = new Match { };

            foreach (var player in jsonResult["info"]["participants"])
            {
                if (player["puuid"] == puuid)
                {
                    match = new Match
                    {
                        MatchID = matchID,
                        Puuid = puuid,
                        ChampionName = player["championName"],
                        ChampionID = player["championId"],
                        Kills = player["kills"],
                        Deaths = player["deaths"],
                        Assists = player["assists"],
                        TotalMinionsKilled = player["totalMinionsKilled"] + player["neutralMinionsKilled"],
                        DamageDealtToTurrets = player["damageDealtToTurrets"],
                        DoubleKills = player["doubleKills"],
                        TripleKills = player["tripleKills"],
                        QuadraKills = player["quadraKills"],
                        PentaKills = player["pentaKills"],
                        TotalDamageDealtToChampions = player["totalDamageDealtToChampions"],
                        TotalDamageShieldedOnTeammates = player["totalDamageShieldedOnTeammates"],
                        TotalHealsOnTeammates = player["totalHealsOnTeammates"],
                        VisionScore = player["visionScore"],
                        VisionWardsBoughtInGame = player["visionWardsBoughtInGame"],
                        WardsPlaced = player["wardsPlaced"],
                        Win = player["win"]
                    };
                    break;
                }
            }

            return match;
        } 
    }
}
