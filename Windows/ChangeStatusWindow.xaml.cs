using System;
using System.Windows;
using VolunteerManagementSystem.Models;

namespace VolunteerManagementSystem.Windows
{
    public partial class ChangeStatusWindow : Window
    {
        private Event _event;

        public ChangeStatusWindow(Event e)
        {
            InitializeComponent();
            _event = e;
            DataContext = new
            {
                EventTitle = e.Title,
                CurrentStatus = e.Status.ToString()
            };
            StatusComboBox.SelectedIndex = (int)e.Status;
            StartDatePicker.SelectedDate = e.StartDate;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            _event.Status = (EventStatus)StatusComboBox.SelectedIndex;
            if (StartDatePicker.SelectedDate.HasValue)
            {
                _event.StartDate = StartDatePicker.SelectedDate.Value;
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
