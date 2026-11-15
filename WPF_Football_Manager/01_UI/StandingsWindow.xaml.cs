using WPF_Football_Manager.BLL;
using WPF_Football_Manager.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Football_Manager.UI
{
    public partial class StandingsWindow : Window
    {
        private readonly LeaguesService _leaguesService = new LeaguesService();
        private readonly StandingsService _standingsService = new StandingsService();
        private bool _isInitializing = true;

        public StandingsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLeaguesComboBox();
        }

        private void LoadLeaguesComboBox()
        {
            try
            {
                _isInitializing = true;

                DataTable leagues = _leaguesService.GetLeagues();
                cboLeagues.ItemsSource = leagues.DefaultView;
                cboLeagues.DisplayMemberPath = "name";
                cboLeagues.SelectedValuePath = "id";

                if (leagues.Rows.Count > 0)
                {
                    cboLeagues.SelectedIndex = 0;
                }

                _isInitializing = false;

                LoadStandings();
            }
            catch (Exception ex)
            {
                _isInitializing = false;
                MessageBox.Show("Грешка при зареждане на лигите: " + ex.Message);
            }
        }

        private void LoadStandings()
        {
            if (cboLeagues.SelectedValue == null)
            {
                return;
            }

            try
            {
                int leagueId = Convert.ToInt32(cboLeagues.SelectedValue);
                List<Standing> standings = _standingsService.GetStandings(leagueId);
                dgvStandings.ItemsSource = standings;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Грешка при изчисляване на класирането: " + ex.Message);
            }
        }

        private void cboLeagues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isInitializing)
            {
                return;
            }

            LoadStandings();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStandings();
            MessageBox.Show(
                "Класирането беше обновено успешно!",
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}