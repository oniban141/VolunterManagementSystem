using System.Collections.Generic;
using System.Windows;
using VolunteerManagementSystem.Models;

namespace VolunteerManagementSystem.Windows
{
    public partial class RegistrationWindow : Window
    {
        public EventRegistration Registration { get; private set; }

        public RegistrationWindow(List<Volunteer> volunteers, List<Event> events)
        {
            InitializeComponent();
            VolunteerComboBox.ItemsSource = volunteers;
            EventComboBox.ItemsSource = events;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (VolunteerComboBox.SelectedValue != null && EventComboBox.SelectedValue != null)
            {
                Registration = new EventRegistration
                {
                    VolunteerId = (int)VolunteerComboBox.SelectedValue,
                    EventId = (int)EventComboBox.SelectedValue
                };

                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите волонтёра и мероприятие.");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
