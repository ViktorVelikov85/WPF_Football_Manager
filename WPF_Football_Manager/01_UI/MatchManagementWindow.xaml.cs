using WPF_Football_Manager.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Football_Manager.UI
{
    public partial class MatchManagementWindow : Window
    {
        private readonly MatchesService _matchesService = new MatchesService();
        private readonly int _matchId;
        private readonly int _homeTeamId;
        private readonly int _awayTeamId;
        private readonly string _homeTeamName;
        private readonly string _awayTeamName;
        private readonly DateTime _matchDate;
        private DataTable? _playersTable;

        public MatchManagementWindow(
            int matchId,
            int homeTeamId,
            int awayTeamId,
            string homeTeamName,
            string awayTeamName,
            DateTime matchDate)
        {
            InitializeComponent();

            _matchId = matchId;
            _homeTeamId = homeTeamId;
            _awayTeamId = awayTeamId;
            _homeTeamName = homeTeamName;
            _awayTeamName = awayTeamName;
            _matchDate = matchDate;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            lblHomeTeam.Text = _homeTeamName;
            lblAwayTeam.Text = _awayTeamName;
            Title = $"Управление на мач: {_homeTeamName} - {_awayTeamName}";

            if (cboEventType.Items.Count > 0)
            {
                cboEventType.SelectedIndex = 0;
            }

            LoadMatchPlayers();
            RefreshEventsAndScore();
        }

        private void LoadMatchPlayers()
        {
            try
            {
                _playersTable = _matchesService.GetPlayersForMatch(_homeTeamId, _awayTeamId);

                cboPlayers.ItemsSource = null;
                cboPlayers.Items.Clear();

                foreach (DataRow row in _playersTable.Rows)
                {
                    string fullName = "";

                    if (_playersTable.Columns.Contains("full_name"))
                    {
                        fullName = row["full_name"]?.ToString() ?? "";
                    }
                    else if (_playersTable.Columns.Contains("first_name") && _playersTable.Columns.Contains("last_name"))
                    {
                        fullName = $"{row["first_name"]} {row["last_name"]}";
                    }
                    else if (_playersTable.Columns.Contains("first_name_bg") || _playersTable.Columns.Contains("last_name_bg"))
                    {
                        fullName = $"{row[1]} {row[2]}";
                    }
                    else
                    {
                        fullName = row[1]?.ToString() ?? "";
                    }

                    cboPlayers.Items.Add(fullName);
                }

                cboPlayers.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при зареждане на футболисти: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void RefreshEventsAndScore()
        {
            try
            {
                DataTable eventsDt = _matchesService.GetMatchEvents(_matchId);
                dgvEvents.ItemsSource = eventsDt.DefaultView;

                lblResult.Text = _matchesService.GetMatchScore(_matchId);

                _matchesService.UpdateMatchResultFromEvents(_matchId, _matchDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при обновяване на хронологията и резултата: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnAddEvent_Click(object sender, RoutedEventArgs e)
        {
            if (cboPlayers.SelectedIndex == -1 || cboEventType.SelectedItem == null)
            {
                MessageBox.Show(
                    "Моля, изберете събитие и футболист!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (_playersTable == null || cboPlayers.SelectedIndex >= _playersTable.Rows.Count)
            {
                return;
            }

            if (!int.TryParse(txtMinute.Text.Trim(), out int minute) || minute < 0 || minute > 120)
            {
                MessageBox.Show(
                    "Моля, въведете валидна минута (от 0 до 120)!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DataRow selectedPlayerRow = _playersTable.Rows[cboPlayers.SelectedIndex];

            string selectedEvent = (cboEventType.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            int playerId = Convert.ToInt32(selectedPlayerRow["id"]);
            int playerClubId = Convert.ToInt32(selectedPlayerRow["club_id"]);

            if (selectedEvent == "Гол")
            {
                _matchesService.AddGoal(_matchId, playerId, playerClubId, minute);
            }
            else if (selectedEvent == "Жълт картон" || selectedEvent == "Червен картон")
            {
                _matchesService.AddCard(_matchId, playerId, selectedEvent, minute);
            }
            else if (selectedEvent == "Фаул")
            {
                _matchesService.AddFoul(_matchId, playerId, minute);
            }

            RefreshEventsAndScore();

            txtMinute.Text = Math.Min(minute + 1, 120).ToString();
        }

        private void btnRemoveEvent_Click(object sender, RoutedEventArgs e)
        {
            if (dgvEvents.SelectedItem is not DataRowView dataRow)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Сигурни ли сте, че искате да изтриете избраното събитие?",
                "Потвърждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                int id = Convert.ToInt32(dataRow["id"]);
                string eventType = dataRow["event_type"]?.ToString() ?? "";

                _matchesService.DeleteEvent(id, eventType, _matchId);
                RefreshEventsAndScore();
            }
        }
    }
}