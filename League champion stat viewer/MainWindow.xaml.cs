using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DAL;
using DAL.Models;

namespace League_champion_stat_viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<Match> Matches = new List<Match>();
        Repository repo = new Repository();
        List<ChampionStats> champions = new List<ChampionStats>();
        string APIKey = "RGAPI-bc491e65-8259-42d1-b896-b3ccd5fbc68c";
        

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            spChampionStats.Visibility = Visibility.Hidden;
            loadingGif.Visibility = Visibility.Collapsed;

            GPButton.Visibility = Visibility.Collapsed;
            WRButton.Visibility = Visibility.Collapsed;
            KDAButton.Visibility = Visibility.Collapsed;
        }

        private async void OnClickShowStats(object sender, RoutedEventArgs e)
        {
            loadingGif.Visibility = Visibility.Visible;
            var puuid = APIMethods.GetSummonerPuuidAsync(SummonerName.Text, serverCode.Text, APIKey);

            try
            {
                await repo.UpdateChampionStatsFromMatches(SummonerName.Text, serverCode.Text, APIKey);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                MessageBox.Show("404, summoner not found");
                return;
            }

            await Grades.UpdateGrades(puuid.Result);

            champions = (await repo.GetAllChampionStatsAsync(puuid.Result)).OrderByDescending(x => x.GamesPlayed).ToList();



            lbChampions.ItemsSource = champions;
            lbChampions.Items.Refresh();
            loadingGif.Visibility = Visibility.Collapsed;

            GPButton.Visibility = Visibility.Visible;
            WRButton.Visibility = Visibility.Visible;
            KDAButton.Visibility = Visibility.Visible;

        }
        private void OnClickSort(object sender, RoutedEventArgs e)
        {
            if (champions.Count == 0)
                return;

            if (sender.Equals(GPButton))
                champions = champions.OrderByDescending(x => x.GamesPlayed).ToList();
            else if (sender.Equals(WRButton))
                champions = champions.OrderByDescending(x => x.WinRate).ToList();
            else if (sender.Equals(KDAButton))
                champions = champions.OrderByDescending(x => x.KDA).ToList();

            lbChampions.ItemsSource = champions;
            lbChampions.Items.Refresh();
        }

        private async void OnClickQuickUpdate(object sender, RoutedEventArgs e)
        {
            spChampionStats.Visibility = Visibility.Hidden;
            await LoadMatchesButtonAsync(20);
            
        }

        private async void OnClickLoadAllMatches(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("This can take up to 30 minutes, proceed?", "Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
                await LoadMatchesButtonAsync(9999);
                
        }

        private async Task LoadMatchesButtonAsync (int count)
        {
            loadingGif.Visibility = Visibility.Visible;
            var loadedSuccessfully = true;
            try
            {
                await repo.LoadMatchesAsync(SummonerName.Text, serverCode.Text, DAL.Models.ServerDict.ServerDictionary[serverCode.Text], APIKey, count);
            }

            catch (System.Net.Http.HttpRequestException exception)
            {
                if ((int)exception.StatusCode == 404)
                {
                    MessageBox.Show("404, summoner not found");
                    loadedSuccessfully = false;
                }
                if ((int)exception.StatusCode == 403)
                {
                    MessageBox.Show("403, empty summoner name or APIKey expired");
                    loadedSuccessfully = false;
                }
                if ((int)exception.StatusCode == 429)
                {
                    MessageBox.Show("429, rate limit exceeded");
                    loadedSuccessfully = false;
                }
            }
            if (loadedSuccessfully)
                MessageBox.Show("Matches loaded successfully");

            loadingGif.Visibility = Visibility.Collapsed;
        }

        private void lbChampionsSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            spChampionStats.Visibility = Visibility.Visible;
            
            var champ = (ChampionStats) lbChampions.SelectedItem;

            if (champ == null)
                return;

            cName.Text = champ.Name;
            cGrade.Text = champ.Grade;
            cGrade.Foreground = GetGradeColor(champ.Grade);

            cGamesPlayed.Text = "Games played: " + champ.GamesPlayed.ToString();
            cWinrate.Text = "Winrate: " + (champ.WinRate * 100).ToString() + "%";

            cKDA.Text = "KDA: " + champ.KDA.ToString();
            cKills.Text = "Kills: " + GetAverage(champ.TotalKills, champ.GamesPlayed);
            cDeaths.Text = "Deaths: " + GetAverage(champ.TotalDeaths, champ.GamesPlayed);
            cAssists.Text = "Assists: " + GetAverage(champ.TotalAssists, champ.GamesPlayed);

            cDouble.Text = "Double kills: " + champ.DoubleKills.ToString();
            cTriple.Text = "Triple kills: " + champ.TripleKills.ToString();
            cQuadra.Text = "Quadra kills: " + champ.QuadraKills.ToString();
            cPenta.Text = "Penta kills: " + champ.PentaKills.ToString();

            cMinionKills.Text = "Creep score: " + GetAverage(champ.TotalMinionsKilled, champ.GamesPlayed);
            cDamageToTurrets.Text = "Damage dealt to turrets: " + GetAverage(champ.TotalDamageDealtToTurrets, champ.GamesPlayed);
            cDamageToChampions.Text = "Damage dealt to champions: " + GetAverage(champ.TotalDamageDealtToChampions, champ.GamesPlayed);
            cShields.Text = "Shields on teammates: " + GetAverage(champ.TotalDamageShieldedOnTeammates, champ.GamesPlayed);
            cHeals.Text = "Heals on teammates: " + GetAverage(champ.TotalHealsOnTeammates, champ.GamesPlayed);

            cWardsPlaced.Text = "Wards placed: " + GetAverage(champ.TotalWardsPlaced, champ.GamesPlayed);
            cVisionScore.Text = "Vision score: " + GetAverage(champ.VisionScoreSum, champ.GamesPlayed);
            cVisionWardsBought.Text = "Vision wards bought: " + GetAverage(champ.TotalVisionWardsBoughtInGame, champ.GamesPlayed);

        }

        private string GetAverage(int stat, int gamesPlayed)
        {
            return Math.Round((stat / (float) gamesPlayed), 2).ToString();
        }

        private Brush GetGradeColor(string grade)
        {
            switch (grade)
            {
                case "S+":
                    return Brushes.Gold;
                case "S":
                    return Brushes.Gold;
                case "A":
                    return Brushes.ForestGreen;
                case "B":
                    return Brushes.DarkGreen;
                case "C":
                    return Brushes.OrangeRed;
                case "D":
                    return Brushes.Crimson;
                default:
                    return Brushes.Gray;
            }
        }
    }
}
