using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using VolunteerManagementSystem.Models;
using VolunteerManagementSystem.Services;
using VolunteerManagementSystem.Windows;


namespace VolunteerManagementSystem
{
    public partial class MainWindow : Window
    {
        private EventRegistrationService _registrationService;
        private List<Volunteer> _volunteers;
        private List<Event> _events;

        public MainWindow()
        {
            InitializeComponent();
            _volunteers = new List<Volunteer>();
            _events = new List<Event>();
            _registrationService = new EventRegistrationService();
            LoadVolunteers();
            LoadEvents();
            LoadRegistrations();
        }

        private void InitializeTestData()
        {
            _volunteers.Clear();
            _events.Clear();
        }


        // В методе ImportVolunteers_Click
        private void ImportVolunteers_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _registrationService.ImportVolunteersFromExcel(openFileDialog.FileName, _volunteers);
                    LoadVolunteers();
                    MessageBox.Show("Волонтёры успешно импортированы!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте волонтёров: {ex.Message}");
                }
            }
        }

        // В методе AddVolunteer_Click
        private void AddVolunteer_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddVolunteerWindow();
            if (addWindow.ShowDialog() == true)
            {
                var nameParts = addWindow.VolunteerName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                string lastName = "", firstName = "", middleName = "";

                if (nameParts.Length > 0) lastName = nameParts[0];
                if (nameParts.Length > 1) firstName = nameParts[1];
                if (nameParts.Length > 2) middleName = nameParts[2];

                if (_volunteers.Any(v => v.LastName == lastName && v.FirstName == firstName && v.MiddleName == middleName))
                {
                    MessageBox.Show("Волонтёр с таким ФИО уже существует!");
                    return;
                }

                _volunteers.Add(new Volunteer
                {
                    Id = Volunteer.GetNextId(_volunteers),
                    LastName = lastName,
                    FirstName = firstName,
                    MiddleName = middleName,
                    Status = VolunteerStatus.Active
                });

                LoadVolunteers();
                MessageBox.Show($"Волонтёр {addWindow.VolunteerName} успешно добавлен!");
            }
        }

        private void ChangeVolunteerStatus_Click(object sender, RoutedEventArgs e)
        {
            if (VolunteersDataGrid.SelectedItem is Volunteer selectedVolunteer)
            {
                var result = MessageBox.Show($"Изменить статус волонтёра {selectedVolunteer.FullName}?\n" +
                                           "Текущий статус: {selectedVolunteer.Status}",
                                           "Изменение статуса",
                                           MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    selectedVolunteer.Status = selectedVolunteer.Status == VolunteerStatus.Active
                        ? VolunteerStatus.Inactive
                        : VolunteerStatus.Active;

                    LoadVolunteers();
                    MessageBox.Show("Статус волонтёра успешно изменён!");
                }
            }
            else
            {
                MessageBox.Show("Выберите волонтёра для изменения статуса.");
            }
        }



        private void ExportAllRegistrations_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = "Все_регистрации.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _registrationService.ExportAllRegistrations(saveFileDialog.FileName, _volunteers, _events);
                    MessageBox.Show($"Все регистрации успешно экспортированы в файл:\n{saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}");
                }
            }
        }


        private void AddEvent_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEventWindow();
            if (addWindow.ShowDialog() == true)
            {
                // Проверка на дублирование
                if (_events.Any(ev => ev.Title.Equals(addWindow.EventTitle, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("Мероприятие с таким названием уже существует!");
                    return;
                }

                // Получаем новый ID
                int newId = _events.Count > 0 ? _events.Max(ev => ev.Id) + 1 : 1;

                // Добавляем новое мероприятие
                _events.Add(new Event
                {
                    Id = newId,
                    Title = addWindow.EventTitle,
                    StartDate = addWindow.EventDate,
                    Status = EventStatus.Planned
                });

                // Обновляем отображение
                LoadEvents();

                MessageBox.Show($"Мероприятие '{addWindow.EventTitle}' успешно добавлено!");
                Console.WriteLine($"Добавлено мероприятие: {addWindow.EventTitle}, ID: {newId}");
                Console.WriteLine($"Всего мероприятий: {_events.Count}");
            }
        }





        private void ChangeEventStatus_Click(object sender, RoutedEventArgs e)
        {
            if (EventsDataGrid.SelectedItem is Event selectedEvent)
            {
                var statusWindow = new ChangeStatusWindow(selectedEvent);
                if (statusWindow.ShowDialog() == true)
                {
                    LoadEvents();
                }
            }
            else
            {
                MessageBox.Show("Выберите мероприятие для изменения статуса.");
            }
        }

        private void RegisterForEvent_Click(object sender, RoutedEventArgs e)
        {
            var registrationWindow = new RegistrationWindow(_volunteers, _events);
            if (registrationWindow.ShowDialog() == true)
            {
                var registration = registrationWindow.Registration;
                var selectedVolunteer = _volunteers.FirstOrDefault(v => v.Id == registration.VolunteerId);

                if (selectedVolunteer == null)
                {
                    MessageBox.Show("Волонтёр не найден!");
                    return;
                }

                if (selectedVolunteer.Status != VolunteerStatus.Active)
                {
                    MessageBox.Show("Нельзя зарегистрировать недействующего волонтёра!");
                    return;
                }

                if (_registrationService.RegisterVolunteerForEvent(registration, _events))
                {
                    LoadRegistrations();
                    MessageBox.Show("Регистрация успешна!");
                }
                else
                {
                    MessageBox.Show("Нельзя зарегистрироваться на это мероприятие!");
                }
            }
        }



        private void ShowStatistics_Click(object sender, RoutedEventArgs e)
        {
            int totalVolunteers = _volunteers.Count;
            int totalEvents = _events.Count;

            // Используем другое имя переменной в лямбда-выражении (например, 'ev')
            int activeEvents = _events.Count(ev => ev.Status == EventStatus.Planned || ev.Status == EventStatus.InProgress);
            int completedEvents = _events.Count(ev => ev.Status == EventStatus.Completed);

            string statistics = $"Статистика:\n\n" +
                              $"Всего волонтёров: {totalVolunteers}\n" +
                              $"Всего мероприятий: {totalEvents}\n" +
                              $"Активных мероприятий: {activeEvents}\n" +
                              $"Завершённых мероприятий: {completedEvents}\n\n" +
                              $"Соотношение волонтёров к мероприятиям: {Math.Round((double)totalVolunteers / totalEvents, 2)}";

            MessageBox.Show(statistics, "Статистика системы");
        }

        private void ImportEvents_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _registrationService.ImportEventsFromExcel(openFileDialog.FileName, _events);
                    LoadEvents();
                    MessageBox.Show("Мероприятия успешно импортированы!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при импорте мероприятий: {ex.Message}");
                }
            }
        }


        private void LoadVolunteers()
        {
            VolunteersDataGrid.ItemsSource = null;
            VolunteersDataGrid.ItemsSource = _volunteers;
        }


        private void LoadEvents()
        {
            EventsDataGrid.ItemsSource = null;
            EventsDataGrid.ItemsSource = _events;
            EventsDataGrid.Items.Refresh();
        }

        private void LoadRegistrations()
        {
            var registrations = _registrationService.GetAllRegistrations();
            var displayData = registrations.Select(r => new
            {
                r.VolunteerId,
                VolunteerName = _volunteers.FirstOrDefault(v => v.Id == r.VolunteerId)?.FullName,
                r.EventId,
                EventName = _events.FirstOrDefault(e => e.Id == r.EventId)?.Title
            }).ToList();

            RegistrationsDataGrid.ItemsSource = displayData;
        }

        private void SaveRegistrationsToExcel_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel файлы (*.xlsx)|*.xlsx",
                FileName = "Регистрации волонтёров.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    _registrationService.SaveToExcel(saveFileDialog.FileName, _volunteers, _events);
                    MessageBox.Show($"Список зарегистрированных волонтёров успешно сохранён в файл:\n{saveFileDialog.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}");
                }
            }
        }


    }
}
