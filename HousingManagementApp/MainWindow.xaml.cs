using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;                // ← важно!

namespace HousingManagementApp
{
    public partial class MainWindow : Window
    {
        private Entities _context = new Entities();

        public MainWindow()
        {
            InitializeComponent();
            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                // В EF6 используем Include для eager loading
                var requests = _context.Request
                    .Include(r => r.Building)
                    .Include(r => r.Inhabitant)
                    .Include(r => r.RequestStatus)   // если Status → RequestStatus
                    .ToList();

                RequestsGrid.ItemsSource = requests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных:\n{ex.Message}",
                               "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Request _selectedRequest;

        private void RequestsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedRequest = RequestsGrid.SelectedItem as Request;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditRequestWindow(null, _context);
            if (editWindow.ShowDialog() == true)
            {
                LoadRequests();
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest == null)
            {
                MessageBox.Show("Выберите заявку", "Внимание");
                return;
            }

            var editWindow = new EditRequestWindow(_selectedRequest, _context);
            if (editWindow.ShowDialog() == true)
            {
                LoadRequests();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRequest == null)
            {
                MessageBox.Show("Выберите заявку", "Внимание");
                return;
            }
            if (MessageBox.Show("Удалить заявку?", "Подтверждение",
                               MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Request.Remove(_selectedRequest);
                    _context.SaveChanges();
                    LoadRequests();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления:\n{ex.Message}", "Ошибка");
                }
            }
        }
        private void HistoryByEmployee_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_context, isByEmployee: true);
            historyWindow.ShowDialog();
        }

        private void HistoryByAddress_Click(object sender, RoutedEventArgs e)
        {
            var historyWindow = new HistoryWindow(_context, isByEmployee: false);
            historyWindow.ShowDialog();
        }
    }
}