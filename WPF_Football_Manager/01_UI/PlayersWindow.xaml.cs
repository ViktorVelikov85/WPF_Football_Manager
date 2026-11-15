using WPF_Football_Manager.BLL;
using WPF_Football_Manager.Models;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WPF_Football_Manager.UI
{
    public partial class PlayersWindow : Window
    {
        private readonly PlayerService _playerService = new PlayerService();
        private readonly ClubService _clubService = new ClubService();
        private int _selectedPlayerId = -1;
        private bool _isInitializing = true;

        public PlayersWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;

            LoadClubsData();
            LoadPlayers();
            ClearInputs();

            cboFilterClub.SelectedIndex = 0;
            cboFilterPosition.SelectedIndex = 0;

            string positionLegend =
                "GK - Вратар (Goalkeeper)\n" +
                "DF - Защитник (Defender)\n" +
                "MF - Полузащитник (Midfielder)\n" +
                "FW - Нападател (Forward)";

            cboPosition.ToolTip = positionLegend;
            cboFilterPosition.ToolTip = positionLegend;

            _isInitializing = false;
        }

        private void LoadClubsData()
        {
            try
            {
                DataTable clubs = _clubService.GetAllClubs();

                cboClub.DisplayMemberPath = "name";
                cboClub.SelectedValuePath = "id";
                cboClub.ItemsSource = clubs.DefaultView;

                DataTable filterClubs = clubs.Copy();
                DataRow row = filterClubs.NewRow();
                row["id"] = 0;
                row["name"] = "Всички";
                filterClubs.Rows.InsertAt(row, 0);

                cboFilterClub.DisplayMemberPath = "name";
                cboFilterClub.SelectedValuePath = "id";
                cboFilterClub.ItemsSource = filterClubs.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при зареждане на клубове: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadPlayers()
        {
            try
            {
                DataTable dt = _playerService.GetPlayers();
                PreparePlayersTable(dt);
                dgvPlayers.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при зареждане на играчи: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PreparePlayersTable(DataTable dt)
        {
            if (!dt.Columns.Contains("status_bg"))
            {
                dt.Columns.Add("status_bg", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                string statusInDb = row["status"]?.ToString() ?? "";

                if (statusInDb == "Active" || statusInDb == "Активен")
                {
                    row["status_bg"] = "Активен";
                }
                else if (statusInDb == "Injured" || statusInDb == "Контузен")
                {
                    row["status_bg"] = "Контузен";
                }
                else if (statusInDb == "Suspended" || statusInDb == "Наказан")
                {
                    row["status_bg"] = "Наказан";
                }
                else
                {
                    row["status_bg"] = statusInDb;
                }
            }
        }

        private void dgvPlayers_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            if (e.Row.Item is not DataRowView dataRow)
            {
                return;
            }

            string statusInDb = dataRow["status"]?.ToString() ?? "";

            if (statusInDb == "Injured" || statusInDb == "Контузен")
            {
                e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 220, 220));
            }
            else if (statusInDb == "Suspended" || statusInDb == "Наказан")
            {
                e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 255, 200));
            }
            else
            {
                e.Row.Background = Brushes.White;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                return;
            }

            var player = MapInputsToModel();

            if (_playerService.SavePlayer(player, true, out string message))
            {
                MessageBox.Show(
                    message,
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadPlayers();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(
                    message,
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayerId == -1)
            {
                MessageBox.Show(
                    "Моля, изберете играч от таблицата!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!ValidateInputs())
            {
                return;
            }

            var player = MapInputsToModel();
            player.Id = _selectedPlayerId;

            if (_playerService.SavePlayer(player, false, out string message))
            {
                MessageBox.Show(
                    message,
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoadPlayers();
                ClearInputs();
            }
            else
            {
                MessageBox.Show(
                    message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayerId == -1)
            {
                MessageBox.Show(
                    "Моля, изберете играч за изтриване!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Сигурни ли сте, че искате да изтриете този играч?",
                "Потвърждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                _playerService.DeletePlayer(_selectedPlayerId);
                LoadPlayers();
                ClearInputs();
            }
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (dgvPlayers.SelectedItem is not DataRowView dataRow)
            {
                return;
            }

            int pId = Convert.ToInt32(dataRow["id"]);
            string pName = dataRow["full_name"]?.ToString() ?? "";
            int? cId = dataRow["club_id"] != DBNull.Value
                ? Convert.ToInt32(dataRow["club_id"])
                : null;
            string cName = dataRow["club_name"]?.ToString() ?? "Свободен агент";

            TransfersWindow transferWnd = new TransfersWindow(pId, pName, cId, cName);
            transferWnd.ShowDialog();
            LoadPlayers();
        }

        private void dgvPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvPlayers.SelectedItem is not DataRowView dataRow)
            {
                return;
            }

            _selectedPlayerId = Convert.ToInt32(dataRow["id"]);
            string fullName = dataRow["full_name"]?.ToString() ?? "";

            string[] names = fullName.Split(new[] { ' ' }, 2);
            txtFirstName.Text = names[0];
            txtLastName.Text = names.Length > 1 ? names[1] : "";

            SelectComboBoxItemByText(cboPosition, dataRow["position"]?.ToString());
            txtShirtNumber.Text = dataRow["shirt_number"]?.ToString() ?? "1";

            if (dataRow["birth_date"] != DBNull.Value)
            {
                dtpBirthDate.SelectedDate = Convert.ToDateTime(dataRow["birth_date"]);
            }

            string dbStatus = dataRow["status"]?.ToString() ?? "";
            string uiStatus = dbStatus;

            if (dbStatus == "Active")
            {
                uiStatus = "Активен";
            }
            else if (dbStatus == "Injured")
            {
                uiStatus = "Контузен";
            }
            else if (dbStatus == "Suspended")
            {
                uiStatus = "Наказан";
            }

            SelectComboBoxItemByText(cboStatus, uiStatus);

            if (dataRow["club_id"] != DBNull.Value)
            {
                cboClub.SelectedValue = Convert.ToInt32(dataRow["club_id"]);
            }
            else
            {
                cboClub.SelectedIndex = -1;
            }
        }

        private void SelectComboBoxItemByText(ComboBox comboBox, string? text)
        {
            if (string.IsNullOrEmpty(text))
            {
                comboBox.SelectedIndex = -1;
                return;
            }

            foreach (var item in comboBox.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Content?.ToString() == text)
                {
                    comboBox.SelectedItem = cbi;
                    return;
                }
            }

            comboBox.SelectedIndex = -1;
        }

        private string GetSelectedComboBoxText(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is ComboBoxItem cbi)
            {
                return cbi.Content?.ToString() ?? "";
            }

            return comboBox.SelectedItem?.ToString() ?? "";
        }

        private void ApplyFilters()
        {
            if (_isInitializing)
            {
                return;
            }

            int? clubId = (cboFilterClub.SelectedValue is int cid && cid > 0)
                ? cid
                : null;

            string pos = GetSelectedComboBoxText(cboFilterPosition);
            if (string.IsNullOrEmpty(pos))
            {
                pos = "Всички";
            }

            DataTable dt = _playerService.GetPlayers(clubId, pos, txtSearchName.Text.Trim());
            PreparePlayersTable(dt);
            dgvPlayers.ItemsSource = dt.DefaultView;
        }

        private void txtSearchName_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cboFilterClub_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void cboFilterPosition_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void btnClearFilters_Click(object sender, RoutedEventArgs e)
        {
            _isInitializing = true;

            txtSearchName.Clear();
            if (cboFilterClub.Items.Count > 0)
            {
                cboFilterClub.SelectedIndex = 0;
            }

            cboFilterPosition.SelectedIndex = 0;

            _isInitializing = false;
            ApplyFilters();
        }

        private Player MapInputsToModel()
        {
            string uiStatus = GetSelectedComboBoxText(cboStatus);
            string dbStatus = "Active";

            if (uiStatus == "Контузен")
            {
                dbStatus = "Injured";
            }
            else if (uiStatus == "Наказан")
            {
                dbStatus = "Suspended";
            }

            int.TryParse(txtShirtNumber.Text.Trim(), out int shirtNumber);

            return new Player
            {
                FullName = $"{txtFirstName.Text.Trim()} {txtLastName.Text.Trim()}",
                ClubId = cboClub.SelectedValue != null ? Convert.ToInt32(cboClub.SelectedValue) : 0,
                BirthDate = dtpBirthDate.SelectedDate ?? DateTime.Now,
                Position = GetSelectedComboBoxText(cboPosition),
                ShirtNumber = shirtNumber,
                Status = dbStatus
            };
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show(
                    "Моля, въведете Име и Фамилия на играча!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (cboPosition.SelectedIndex == -1 || cboClub.SelectedValue == null || cboStatus.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Моля, направете избор от всички падащи менюта!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            if (!int.TryParse(txtShirtNumber.Text.Trim(), out int num) || num < 1 || num > 99)
            {
                MessageBox.Show(
                    "Моля, въведете валиден номер на тениската (от 1 до 99)!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ClearInputs()
        {
            txtFirstName.Clear();
            txtLastName.Clear();
            txtShirtNumber.Text = "1";
            dtpBirthDate.SelectedDate = DateTime.Now;
            cboPosition.SelectedIndex = -1;
            cboClub.SelectedIndex = -1;
            cboStatus.SelectedIndex = 0;

            _selectedPlayerId = -1;
            dgvPlayers.SelectedItem = null;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }
    }
}