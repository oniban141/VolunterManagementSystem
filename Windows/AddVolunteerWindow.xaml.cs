using System.Windows;

namespace VolunteerManagementSystem.Windows
{
    public partial class AddVolunteerWindow : Window
    {
        public string VolunteerName { get; private set; }

        public AddVolunteerWindow()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                VolunteerName = NameTextBox.Text;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, введите ФИО волонтёра.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
