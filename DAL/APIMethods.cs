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
    public static class APIMethods
    {
        // Loads the API key for use in requests
        // developer riot API key lasts for 24 hours
        // so it needs to be changed daily
        private static readonly string APIKey = File.ReadAllText("Config\\riotApiKey.txt");

        // general GET request function, returns the request result as string
        public static async Task<string> GetAsync(string url)
        {
            using var client = new HttpClient();
            var content = await client.GetStringAsync(url);
            return content;
        }

        // Returns player's puuid
        public static async Task<string> GetSummonerPuuidAsync(string summonerName, string serverName)
        {
            var uri = $"https://{serverName}.api.riotgames.com/lol/summoner/v4/summoners/by-name/{summonerName}?api_key={APIKey}";
            var result = await GetAsync(uri);
            dynamic jsonResult = JsonConvert.DeserializeObject(result);
            return jsonResult["puuid"];
        }

        // Returns all match IDs that were returned by the request
        public static async Task<List<string>> GetSummonerMatchIDsAsync(string region, string  puuid)
        {
            var matchIds = new List<string>();
            
            var index = 0;
            bool stop = false;

            // Load match IDs as long as the request returns 100 match IDs
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

        // Send a request for a single match and construct a match from recieved data
        public static async Task<Match> GetMatchStatsAsync(string region, string puuid, string matchID)
        {
            var uri = $"https://{region}.api.riotgames.com/lol/match/v5/matches/{matchID}?api_key={APIKey}";

            var result = await GetAsync(uri);
            dynamic jsonResult = JsonConvert.DeserializeObject(result);
            var match = new Match { };

            // find the played by puuid in participants list
            foreach (var player in jsonResult["info"]["participants"])
            {

                if (player["puuid"] == puuid)
                {
                    // construct a match from match's data
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
