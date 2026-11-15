using WPF_Football_Manager.BLL;
using WPF_Football_Manager.Models;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WPF_Football_Manager.UI
{
    public partial class LeaguesWindow : Window
    {
        private readonly LeaguesService _leagueService = new LeaguesService();
        private int _selectedLeagueId = -1;

        public LeaguesWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLeagues();
        }

        // Управление на лиги
        private void LoadLeagues()
        {
            try
            {
                DataTable dt = _leagueService.GetLeagues();
                dgvLeagues.ItemsSource = dt.DefaultView;

                if (dt.Rows.Count > 0)
                {
                    dgvLeagues.SelectedIndex = 0;
                }
                else
                {
                    ClearInputs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Грешка при зареждане на лиги: " + ex.Message);
            }
        }

        private void dgvLeagues_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvLeagues.SelectedItem is not DataRowView dataRow)
            {
                return;
            }

            _selectedLeagueId = Convert.ToInt32(dataRow["id"]);
            txtName.Text = dataRow["name"]?.ToString() ?? "";
            txtSeason.Text = dataRow["season"]?.ToString() ?? "";

            RefreshParticipants();
            RefreshSchedule();
        }

        private void btnAddLeague_Click(object sender, RoutedEventArgs e)
        {
            var league = new League
            {
                Name = txtName.Text.Trim(),
                Season = txtSeason.Text.Trim()
            };

            if (_leagueService.SaveLeague(league, true, out string msg))
            {
                MessageBox.Show(msg);
                LoadLeagues();
            }
            else
            {
                MessageBox.Show(
                    msg,
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void btnUpdateLeague_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLeagueId == -1)
            {
                return;
            }

            var league = new League
            {
                Id = _selectedLeagueId,
                Name = txtName.Text.Trim(),
                Season = txtSeason.Text.Trim()
            };

            if (_leagueService.SaveLeague(league, false, out string msg))
            {
                MessageBox.Show(msg);
                LoadLeagues();
            }
            else
            {
                MessageBox.Show(
                    msg,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnDeleteLeague_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLeagueId == -1)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Изтриване на лигата и всички мачове към нея?",
                "Внимание",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _leagueService.DeleteLeague(_selectedLeagueId);
                _selectedLeagueId = -1;
                LoadLeagues();
            }
        }

        // Управление на участници (клубове в лигата)
        private void RefreshParticipants()
        {
            if (_selectedLeagueId == -1)
            {
                return;
            }

            DataTable participants = _leagueService.GetParticipants(_selectedLeagueId);
            dgvParticipants.ItemsSource = participants.DefaultView;

            DataTable available = _leagueService.GetAvailableClubs(_selectedLeagueId);
            cboAvailableClubs.ItemsSource = available.DefaultView;
            cboAvailableClubs.DisplayMemberPath = "name";
            cboAvailableClubs.SelectedValuePath = "id";
        }

        private void btnAddClub_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLeagueId == -1 || cboAvailableClubs.SelectedValue == null)
            {
                return;
            }

            int clubId = Convert.ToInt32(cboAvailableClubs.SelectedValue);
            _leagueService.AddClubToLeague(_selectedLeagueId, clubId);
            RefreshParticipants();
        }

        private void btnRemoveClub_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLeagueId == -1 || dgvParticipants.SelectedItem is not DataRowView partRow)
            {
                return;
            }

            int clubId = Convert.ToInt32(partRow["id"]);
            _leagueService.RemoveClubFromLeague(_selectedLeagueId, clubId);
            RefreshParticipants();
        }

        // Генератор и управление на срещи
        private void RefreshSchedule()
        {
            if (_selectedLeagueId == -1)
            {
                return;
            }

            DataTable schedule = _leagueService.GetSchedule(_selectedLeagueId);
            dgvSchedule.ItemsSource = schedule.DefaultView;
        }

        private void btnGenerateSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLeagueId == -1)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Генериране на нова програма (всяка събота)? Старата ще се изтрие!",
                "Потвърждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                if (_leagueService.GenerateFullSchedule(_selectedLeagueId, out string msg))
                {
                    MessageBox.Show(
                        msg,
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    RefreshSchedule();
                }
                else
                {
                    MessageBox.Show(
                        msg,
                        "Грешка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void dgvSchedule_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgvSchedule.SelectedItem != null)
            {
                OpenMatchManagement();
            }
        }

        private void btnManageMatch_Click(object sender, RoutedEventArgs e)
        {
            OpenMatchManagement();
        }

        private void OpenMatchManagement()
        {
            if (dgvSchedule.SelectedItem is not DataRowView matchRow)
            {
                MessageBox.Show(
                    "Моля, първо изберете мач от програмата!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            int matchId = Convert.ToInt32(matchRow["id"]);
            int homeTeamId = Convert.ToInt32(matchRow["home_team_id"]);
            int awayTeamId = Convert.ToInt32(matchRow["away_team_id"]);

            string homeTeamName = matchRow["home_team"]?.ToString() ?? "";
            string awayTeamName = matchRow["away_team"]?.ToString() ?? "";

            DateTime matchDate = matchRow["match_date"] != DBNull.Value
                ? Convert.ToDateTime(matchRow["match_date"])
                : DateTime.Today;

            MatchManagementWindow wnd = new MatchManagementWindow(
                matchId,
                homeTeamId,
                awayTeamId,
                homeTeamName,
                awayTeamName,
                matchDate);
            wnd.ShowDialog();
            RefreshSchedule();
        }

        private void ClearInputs()
        {
            _selectedLeagueId = -1;
            txtName.Clear();
            txtSeason.Clear();
            dgvParticipants.ItemsSource = null;
            dgvSchedule.ItemsSource = null;
            dgvLeagues.SelectedItem = null;
            txtName.Focus();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }
    }
}