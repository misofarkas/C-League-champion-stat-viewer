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

namespace League_champion_stat_viewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //List<Match> Matches = new List<Match>();
        Repository repo = new Repository();
        string APIKey = "RGAPI-2cac2d09-d16d-4be7-9a35-008959eae621";
        public MainWindow()
        {
            InitializeComponent();
            //Matches = repo.GetAllMatches().Result;
            
        }

        private async void OnClickSearchButton(object sender, RoutedEventArgs e)
        {
            // Last match id --> EUN1_2836268259
            var puuid = APIMethods.GetSummonerPuuidAsync(SummonerName.Text, "EUN1", APIKey);
            await repo.UpdateChampionStatsFromMatches(SummonerName.Text, "EUN1", APIKey);
            //await repo.UpdatePlayerMatchIDs("testplayerPUUID", "EUN1_2836268259", "EUN1_2736268259");
            //await repo.UpdatePlayerMatchIDs("testplayerPUUID", "EUN1_2936268259", "EUN1_2836268259");
            var champions = (await repo.GetAllChampionStatsAsync(puuid.Result)).OrderByDescending(x => x.GamesPlayed).ToList();
            icChampions.ItemsSource = champions;
            icChampions.Items.Refresh();
            

            MessageBox.Show($"{champions.Count} Champions displayed");
        }

        private async void OnClickLoad100Matches(object sender, RoutedEventArgs e)
        {
            await LoadMatchesButtonAsync(100);
        }

        private async void OnClickLoadAllMatches(object sender, RoutedEventArgs e)
        {
            await LoadMatchesButtonAsync(9999);
        }

        private async Task LoadMatchesButtonAsync (int count)
        {
            var loadedSuccessfully = true;
            try
            {
                await repo.LoadMatchesAsync(SummonerName.Text, "EUN1", "EUROPE", APIKey, 200);
            }

            catch (System.Net.Http.HttpRequestException exception)
            {
                if ((int)exception.StatusCode == 404)
                {
                    MessageBox.Show("404, one or more matches not found");
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
        }
    }
}
