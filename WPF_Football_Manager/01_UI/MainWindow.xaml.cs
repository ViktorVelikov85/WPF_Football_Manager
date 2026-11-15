using System.Windows;

namespace WPF_Football_Manager.UI
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnOpenClubs_Click(object sender, RoutedEventArgs e)
        {
            ClubsWindow clubsWindow = new ClubsWindow();
            clubsWindow.ShowDialog();
        }

        private void btnOpenLeagues_Click(object sender, RoutedEventArgs e)
        {
            LeaguesWindow leaguesWindow = new LeaguesWindow();
            leaguesWindow.ShowDialog();
        }

        private void btnOpenPlayers_Click(object sender, RoutedEventArgs e)
        {
            PlayersWindow playersWindow = new PlayersWindow();
            playersWindow.ShowDialog();
        }

        private void btnOpenTransfers_Click(object sender, RoutedEventArgs e)
        {
            TransfersHistoryWindow transfersHistoryWindow = new TransfersHistoryWindow();
            transfersHistoryWindow.ShowDialog();
        }

        private void btnOpenStandings_Click(object sender, RoutedEventArgs e)
        {
            StandingsWindow standingsWindow = new StandingsWindow();
            standingsWindow.ShowDialog();
        }

        private void btnOpenStatistics_Click(object sender, RoutedEventArgs e)
        {
            PlayersStatisticsWindow statisticsWindow = new PlayersStatisticsWindow();
            statisticsWindow.ShowDialog();
        }
    }
}