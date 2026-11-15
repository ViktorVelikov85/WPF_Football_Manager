using WPF_Football_Manager.BLL;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Football_Manager.UI
{
    public partial class TransfersWindow : Window
    {
        private readonly PlayerService _playerService = new PlayerService();
        private readonly ClubService _clubService = new ClubService();
        private int _selectedPlayerId = -1;

        public TransfersWindow()
        {
            InitializeComponent();
        }

        public TransfersWindow(int playerId, string playerName, int? currentClubId, string currentClubName)
        {
            InitializeComponent();

            _selectedPlayerId = playerId;
            txtPlayer.Text = playerName;
            txtFromClub.Text = string.IsNullOrEmpty(currentClubName)
                ? "Free Agent"
                : currentClubName;
            txtFromClub.Tag = currentClubId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClubs();
            RefreshGrid();

            dtpTransferDate.SelectedDate = DateTime.Now;
            txtPlayer.IsReadOnly = true;
            txtFromClub.IsReadOnly = true;
        }

        private void LoadClubs()
        {
            try
            {
                DataTable clubs = _clubService.GetAllClubs();

                cboToClub.ItemsSource = clubs.DefaultView;
                cboToClub.DisplayMemberPath = "name";
                cboToClub.SelectedValuePath = "id";
                cboToClub.SelectedIndex = -1;
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

        private void RefreshGrid()
        {
            try
            {
                string search = txtSearchNameTransfer.Text.Trim();
                DataTable dt = _playerService.GetTransferHistory(search);
                dgvTransfers.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при зареждане на трансферите: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void txtSearchNameTransfer_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshGrid();
        }

        private void ClearInputs()
        {
            cboToClub.SelectedIndex = -1;
            txtFee.Text = "0";
            dtpTransferDate.SelectedDate = DateTime.Now;
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }

        private void btnTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPlayerId == -1)
            {
                MessageBox.Show(
                    "Моля, първо изберете играч от мениджмънта на играчи!",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (cboToClub.SelectedIndex == -1 || cboToClub.SelectedValue == null)
            {
                MessageBox.Show(
                    "Моля, изберете нов клуб!",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            int toClubId = Convert.ToInt32(cboToClub.SelectedValue);
            int? fromClubId = txtFromClub.Tag as int?;

            if (fromClubId.HasValue && fromClubId.Value == toClubId)
            {
                MessageBox.Show(
                    "Играчът вече е в този клуб! Изберете различен отбор.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string feeText = txtFee.Text.Trim().Replace(',', '.');
            if (!decimal.TryParse(feeText, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal fee) || fee < 0)
            {
                MessageBox.Show(
                    "Моля, въведете валидна сума за трансфер!",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DateTime transferDate = dtpTransferDate.SelectedDate ?? DateTime.Now;

            try
            {
                _playerService.ExecuteTransfer(_selectedPlayerId, fromClubId, toClubId, transferDate, fee);

                MessageBox.Show(
                    "Трансферът беше извършен успешно!",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                RefreshGrid();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Грешка при изпълнение на трансфера: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}