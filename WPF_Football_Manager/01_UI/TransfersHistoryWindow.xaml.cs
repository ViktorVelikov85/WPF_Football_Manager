using WPF_Football_Manager.BLL;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Football_Manager.UI
{
    public partial class TransfersHistoryWindow : Window
    {
        private readonly PlayerService _playerService = new PlayerService();

        public TransfersHistoryWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshGrid();
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
                    "Грешка при зареждане на историята: " + ex.Message,
                    "Грешка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void txtSearchNameTransfer_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshGrid();
        }
    }
}