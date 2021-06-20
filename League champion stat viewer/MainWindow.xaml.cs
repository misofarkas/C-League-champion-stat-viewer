using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DAL;
using DAL.Models;
using DAL.Export;
using Microsoft.Win32;

namespace League_champion_stat_viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        List<ChampionStats> champions = new List<ChampionStats>();

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;

            spChampionStats.Visibility = Visibility.Hidden;
            loadingGif.Visibility = Visibility.Collapsed;

            GPButton.Visibility = Visibility.Collapsed;
            WRButton.Visibility = Visibility.Collapsed;
            KDAButton.Visibility = Visibility.Collapsed;
            ExportButton.Visibility = Visibility.Collapsed;
        }

        // Updates champion stats from matches, updtates champion's grades and
        // displays champions in champion ListBox
        private async void OnClickShowStats(object sender, RoutedEventArgs e)
        {
            loadingGif.Visibility = Visibility.Visible;

            string puuid;
            try
            {
                puuid = await APIMethods.GetSummonerPuuidAsync(SummonerName.Text, serverCode.Text);
                await Repository.UpdateChampionStatsFromMatches(SummonerName.Text, serverCode.Text);
            }
            catch (System.Net.Http.HttpRequestException exception)
            {
                HandleException(exception);
                return;
            }

            await Grades.UpdateGrades(puuid);

            champions = (await Repository.GetAllChampionStatsAsync(puuid)).OrderByDescending(x => x.GamesPlayed).ToList();

            lbChampions.ItemsSource = champions;
            lbChampions.Items.Refresh();
            loadingGif.Visibility = Visibility.Collapsed;

            GPButton.Visibility = Visibility.Visible;
            WRButton.Visibility = Visibility.Visible;
            KDAButton.Visibility = Visibility.Visible;
            ExportButton.Visibility = Visibility.Visible;

        }

        // Can be triggered by Games played button, Winrate button and KDA button above champion ListBox
        // Orders the champion by a chosen property and updates the content of champion ListBox
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

        // Loads the last 20 matches for a given player into DB
        private async void OnClickQuickUpdate(object sender, RoutedEventArgs e)
        {
            await LoadMatchesButtonAsync(20);
        }

        // Loads all matches for a given player into DB
        private async void OnClickLoadAllMatches(object sender, RoutedEventArgs e)
        {
            var messageBoxResult = MessageBox.Show("This can take up to 30 minutes, proceed?", "Confirmation", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
                await LoadMatchesButtonAsync(9999);
                
        }

        // Loads matches into DB
        private async Task LoadMatchesButtonAsync (int count)
        {
            loadingGif.Visibility = Visibility.Visible;
            var loadedSuccessfully = true;
            try
            {
                await Repository.LoadMatchesAsync(SummonerName.Text, serverCode.Text, ServerDict.ServerDictionary[serverCode.Text], count);
            }

            catch (System.Net.Http.HttpRequestException exception)
            {
                loadedSuccessfully = false;
                HandleException(exception);
            }

            loadingGif.Visibility = Visibility.Collapsed;
            if (loadedSuccessfully)
                MessageBox.Show("Matches loaded successfully");

        }

        // Handles exceptions that can arise from API
        private void HandleException(System.Net.Http.HttpRequestException exception)
        {
            loadingGif.Visibility = Visibility.Collapsed;
            if ((int)exception.StatusCode == 404)
            {
                MessageBox.Show("404, summoner not found");
            }
            if ((int)exception.StatusCode == 403)
            {
                MessageBox.Show("403, empty summoner name or APIKey expired");
            }
            if ((int)exception.StatusCode == 429)
            {
                MessageBox.Show("429, rate limit exceeded");
            }
        }

        // Exports the current displayed champion list as a CSV file
        private async void OnClickExport(object sender, RoutedEventArgs e)
        {
            loadingGif.Visibility = Visibility.Visible;

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                ExportCSV.Export(champions, saveFileDialog.FileName);
            }

            loadingGif.Visibility = Visibility.Collapsed;
        }

        // Reacts to a change in champion ListBox selection
        // Displays more in-depth stats about a selected champion
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
