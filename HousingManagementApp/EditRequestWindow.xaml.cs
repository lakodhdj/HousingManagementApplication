using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;

namespace HousingManagementApp
{
    public partial class EditRequestWindow : Window
    {
        private readonly Entities _context;
        private Request _request;
        private readonly bool _isNew;

        public EditRequestWindow(Request request, Entities context)
        {
            InitializeComponent();
            _context = context;
            _request = request ?? new Request { CreationDate = DateTime.Now };
            _isNew = request == null;

            LoadComboBoxes();

            if (!_isNew)
            {
                // Загружаем связанные сущности
                _context.Entry(_request).Reference(r => r.Building).Load();
                _context.Entry(_request).Reference(r => r.Inhabitant).Load();
                _context.Entry(_request).Reference(r => r.RequestStatus).Load();

                BuildingCombo.SelectedValue = _request.BuildingId;
                InhabitantCombo.SelectedValue = _request.InhabitantId;
                PhoneText.Text = _request.Inhabitant?.PhoneNum ?? "";
                DescriptionText.Text = _request.RequestText;
                StatusCombo.SelectedValue = _request.StatusId;

                // Первый исполнитель (для простоты)
                var execution = _context.RequestExecution
                    .Include(re => re.WorkingGroup.Select(wg => wg.Employee))
                    .FirstOrDefault(re => re.RequestId == _request.Id);

                if (execution?.WorkingGroup.Any() == true)
                {
                    EmployeeCombo.SelectedValue = execution.WorkingGroup.First().EmployeeId;
                }
            }
        }

        private void LoadComboBoxes()
        {
            BuildingCombo.ItemsSource = _context.Building.ToList();
            BuildingCombo.DisplayMemberPath = "Address";
            BuildingCombo.SelectedValuePath = "Id";

            InhabitantCombo.ItemsSource = _context.Inhabitant.ToList();
            InhabitantCombo.DisplayMemberPath = "FullName";
            InhabitantCombo.SelectedValuePath = "Id";

            EmployeeCombo.ItemsSource = _context.Employee.Where(e => e.RoleId == 3).ToList();
            EmployeeCombo.DisplayMemberPath = "FullName";
            EmployeeCombo.SelectedValuePath = "Id";

            StatusCombo.ItemsSource = _context.RequestStatus.ToList();
            StatusCombo.DisplayMemberPath = "Description";
            StatusCombo.SelectedValuePath = "Id";
            StatusCombo.SelectedIndex = 0;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AllFieldsFilled())
            {
                MessageBox.Show("Заполните все обязательные поля!", "Ошибка");
                return;
            }

            try
            {
                _request.BuildingId = (int)BuildingCombo.SelectedValue;
                _request.InhabitantId = (int)InhabitantCombo.SelectedValue;
                _request.RequestText = DescriptionText.Text;
                _request.StatusId = (int)StatusCombo.SelectedValue;

                if (_isNew)
                {
                    _context.Request.Add(_request);
                }

                _context.SaveChanges();

                // Работа с исполнением и группой (упрощённо — один исполнитель)
                var execution = _context.RequestExecution
                    .FirstOrDefault(re => re.RequestId == _request.Id);

                if (execution == null)
                {
                    execution = new RequestExecution
                    {
                        RequestId = _request.Id,
                        DateStart = DateTime.Now
                    };
                    _context.RequestExecution.Add(execution);
                    _context.SaveChanges();
                }

                var worker = _context.WorkingGroup
                    .FirstOrDefault(w => w.RequestExecutionId == execution.Id);

                if (worker == null)
                {
                    worker = new WorkingGroup { RequestExecutionId = execution.Id };
                    _context.WorkingGroup.Add(worker);
                }

                worker.EmployeeId = (int)EmployeeCombo.SelectedValue;

                _context.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения:\n{ex.Message}", "Ошибка");
            }
        }

        private bool AllFieldsFilled()
        {
            return BuildingCombo.SelectedValue != null &&
                   InhabitantCombo.SelectedValue != null &&
                   EmployeeCombo.SelectedValue != null &&
                   StatusCombo.SelectedValue != null &&
                   !string.IsNullOrWhiteSpace(DescriptionText.Text);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}