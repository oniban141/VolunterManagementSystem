using System;
using System.Windows;

namespace VolunteerManagementSystem.Windows
{
    public partial class AddEventWindow : Window
    {
        public string EventTitle { get; private set; }
        public DateTime EventDate { get; private set; }

        public AddEventWindow()
        {
            InitializeComponent();
            DatePicker.SelectedDate = DateTime.Today.AddDays(7);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TitleTextBox.Text) && DatePicker.SelectedDate.HasValue)
            {
                EventTitle = TitleTextBox.Text;
                EventDate = DatePicker.SelectedDate.Value;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
