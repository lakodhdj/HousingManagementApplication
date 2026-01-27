using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace HousingManagementApp
{
    public partial class HistoryWindow : Window
    {
        private readonly Entities _context;
        private readonly bool _isByEmployee;

        // Загружаем данные один раз при открытии окна
        private List<RequestExecution> _allExecutions;

        public HistoryWindow(Entities context, bool isByEmployee)
        {
            InitializeComponent();

            _context = context;
            _isByEmployee = isByEmployee;

            TitleText.Text = _isByEmployee
                ? "История по Сотрудникам"
                : "История по Адресам";

            LoadDataAndFilter();
        }

        private void LoadDataAndFilter()
        {
            try
            {
                // Загружаем все нужные связи сразу
                _allExecutions = _context.RequestExecution
                    .Include("Request.Building")
                    .Include("Request")
                    .Include("WorkingGroup.Employee")      // если это имя не работает — см. ниже варианты
                                                            // .Include("WorkingGroup.Employee")     // ← попробуйте этот вариант, если предыдущий падает
                                                            // .Include("WorkingGroup1.Employee")    // ← или этот, если EDMX сгенерировал странное имя
                    .ToList();

                // Если загрузка прошла успешно — сразу показываем все записи (или пусто)
                HistoryGrid.ItemsSource = _allExecutions;

                LoadFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных:\n" + ex.Message,
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFilter()
        {
            if (_isByEmployee)
            {
                FilterCombo.ItemsSource = _context.Employee.ToList();
                FilterCombo.DisplayMemberPath = "FullName";
                FilterCombo.SelectedValuePath = "Id";
            }
            else
            {
                FilterCombo.ItemsSource = _context.Building.ToList();
                FilterCombo.DisplayMemberPath = "Address";
                FilterCombo.SelectedValuePath = "Id";
            }

            // Можно сразу выбрать первый элемент, чтобы показать данные
            if (FilterCombo.Items.Count > 0)
            {
                FilterCombo.SelectedIndex = 0;
            }
        }

        private void FilterCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterCombo.SelectedValue == null || _allExecutions == null)
                return;

            var filterId = (int)FilterCombo.SelectedValue;

            IEnumerable<RequestExecution> filtered;

            if (_isByEmployee)
            {
                filtered = _allExecutions.Where(re =>
                    re.WorkingGroup != null &&
                    re.WorkingGroup.Any(wg => wg.EmployeeId == filterId));
            }
            else
            {
                filtered = _allExecutions.Where(re =>
                    re.Request != null &&
                    re.Request.BuildingId == filterId);
            }

            HistoryGrid.ItemsSource = filtered.ToList();
        }
    }
}