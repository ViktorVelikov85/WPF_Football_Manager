using WPF_Football_Manager.BLL;
using WPF_Football_Manager.Models;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace WPF_Football_Manager.UI
{
    public partial class ClubsWindow : Window
    {
        private readonly ClubService _service = new ClubService();
        private int _selectedId = -1;

        public ClubsWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadClubs();
            ClearInputs();
        }

        private void LoadClubs()
        {
            try
            {
                DataTable dt = _service.GetAllClubs();
                dgvClubs.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Грешка при зареждане на клубове: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Моля, въведете име на отбор!");
                return;
            }

            var club = MapInputsToModel();
            if (_service.SaveClub(club, true, out string message))
            {
                FinishOperation(message);
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == -1)
            {
                MessageBox.Show("Моля, изберете клуб от списъка!");
                return;
            }

            var club = MapInputsToModel();
            club.Id = _selectedId;

            if (_service.SaveClub(club, false, out string message))
            {
                FinishOperation(message);
            }
            else
            {
                MessageBox.Show(message);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == -1)
            {
                MessageBox.Show(
                    "Моля, изберете клуб от таблицата за изтриване!",
                    "Внимание",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string clubName = "този отбор";
            if (dgvClubs.SelectedItem is DataRowView selectedRow)
            {
                clubName = selectedRow["name"]?.ToString() ?? "този отбор";
            }

            string confirmMessage =
                $"ВНИМАНИЕ!\n\nСигурни ли сте, че искате да изтриете отбор '{clubName}'?\n\n" +
                "Това действие е необратимо и ЩЕ ИЗТРИЕ ОЩЕ:\n" +
                "• Всички мачове, в които отборът участва (домакин или гост)\n" +
                "• Всички футболисти, играещи в този отбор\n" +
                "• Статистиката за голове, картони и фалове от тези мачове";

            MessageBoxResult result = MessageBox.Show(
                confirmMessage,
                "Потвърждение за ИЗТРИВАНЕ",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _service.DeleteClub(_selectedId);
                    FinishOperation("Клубът и всички свързани с него мачове и играчи бяха изтрити успешно!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Грешка при изтриване: " + ex.Message,
                        "Грешка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void dgvClubs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgvClubs.SelectedItem is not DataRowView dataRow)
            {
                return;
            }

            _selectedId = Convert.ToInt32(dataRow["id"]);

            txtName.Text = dataRow["name"]?.ToString() ?? "";
            txtCity.Text = dataRow["city"]?.ToString() ?? "";
            txtStadium.Text = dataRow["stadium"]?.ToString() ?? "";
            txtCreatedIn.Text = dataRow["founded_year"]?.ToString() ?? "";
        }

        private Club MapInputsToModel()
        {
            return new Club
            {
                Name = txtName.Text.Trim(),
                City = txtCity.Text.Trim(),
                Stadium = txtStadium.Text.Trim(),
                FoundedYear = txtCreatedIn.Text.Trim()
            };
        }

        private void FinishOperation(string message)
        {
            MessageBox.Show(
                message,
                "Информация",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            LoadClubs();
            ClearInputs();
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtCity.Clear();
            txtStadium.Clear();
            txtCreatedIn.Clear();

            _selectedId = -1;
            dgvClubs.SelectedItem = null;

            txtName.Focus();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearInputs();
        }
    }
}