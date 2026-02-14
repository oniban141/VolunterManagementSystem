using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VolunteerManagementSystem.Models;

namespace VolunteerManagementSystem.Services
{
    public class EventRegistrationService
    {
        private List<EventRegistration> _registrations = new List<EventRegistration>();

        public bool RegisterVolunteerForEvent(EventRegistration registration, List<Event> events)
        {
            var selectedEvent = events.FirstOrDefault(ev => ev.Id == registration.EventId);
            if (selectedEvent == null ||
                selectedEvent.Status == EventStatus.Completed ||
                selectedEvent.Status == EventStatus.Cancelled)
            {
                return false;
            }

            if (_registrations.Any(r => r.VolunteerId == registration.VolunteerId &&
                                      r.EventId == registration.EventId))
            {
                return false;
            }

            _registrations.Add(registration);
            return true;
        }

        public List<EventRegistration> GetAllRegistrations()
        {
            return _registrations.ToList();
        }

        public void ImportVolunteersFromExcel(string filePath, List<Volunteer> volunteers)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheetAt(0);

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    var lastName = row.GetCell(0)?.ToString();
                    var firstName = row.GetCell(1)?.ToString();
                    var middleName = row.GetCell(2)?.ToString();

                    if (string.IsNullOrWhiteSpace(lastName) && string.IsNullOrWhiteSpace(firstName)) continue;

                    var exists = volunteers.Any(v =>
                        v.LastName == lastName &&
                        v.FirstName == firstName &&
                        v.MiddleName == middleName);

                    if (!exists)
                    {
                        volunteers.Add(new Volunteer
                        {
                            Id = Volunteer.GetNextId(volunteers),
                            LastName = lastName ?? "",
                            FirstName = firstName ?? "",
                            MiddleName = middleName ?? "",
                            Status = VolunteerStatus.Active
                        });
                    }
                }
            }
        }

        public void ImportEventsFromExcel(string filePath, List<Event> events)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook;
                try
                {
                    workbook = new XSSFWorkbook(fs); // Для .xlsx
                }
                catch
                {
                    fs.Position = 0;
                    workbook = new HSSFWorkbook(fs); // Для .xls
                }

                ISheet sheet = workbook.GetSheetAt(0);

                // Проверяем заголовки
                var header = sheet.GetRow(0);
                int titleCol = -1, startDateCol = -1, endDateCol = -1, statusCol = -1;
                for (int i = 0; i < header.Cells.Count; i++)
                {
                    var cell = header.GetCell(i);
                    if (cell == null) continue;

                    switch (cell.ToString().ToLower())
                    {
                        case "название":
                        case "title":
                            titleCol = i;
                            break;
                        case "дата начала":
                        case "start date":
                            startDateCol = i;
                            break;
                        case "дата окончания":
                        case "end date":
                            endDateCol = i;
                            break;
                        case "статус":
                        case "status":
                            statusCol = i;
                            break;
                    }
                }

                if (titleCol == -1 || startDateCol == -1)
                {
                    throw new Exception("Не найдены обязательные столбцы: Название и Дата начала");
                }

                for (int i = 1; i <= sheet.LastRowNum; i++)
                {
                    var row = sheet.GetRow(i);
                    if (row == null) continue;

                    // Получаем значения из ячеек
                    var titleCell = row.GetCell(titleCol);
                    var startDateCell = row.GetCell(startDateCol);
                    var endDateCell = endDateCol != -1 ? row.GetCell(endDateCol) : null;
                    var statusCell = statusCol != -1 ? row.GetCell(statusCol) : null;

                    var title = titleCell?.ToString();
                    var startDateStr = startDateCell?.ToString();
                    var endDateStr = endDateCell?.ToString();
                    var statusStr = statusCell?.ToString();

                    // Проверяем обязательные поля
                    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(startDateStr)) continue;

                    // Парсим даты
                    if (!DateTime.TryParse(startDateStr, out var startDate)) continue;
                    DateTime endDate = startDate; // По умолчанию = дате начала
                    if (!string.IsNullOrWhiteSpace(endDateStr) && !DateTime.TryParse(endDateStr, out endDate))
                    {
                        endDate = startDate; // Если дата окончания некорректна, используем дату начала
                    }

                    // Определяем статус
                    EventStatus status = EventStatus.Planned; // По умолчанию
                    if (!string.IsNullOrWhiteSpace(statusStr))
                    {
                        switch (statusStr.ToLower())
                        {
                            case "запланировано":
                            case "planned":
                                status = EventStatus.Planned;
                                break;
                            case "идёт":
                            case "in progress":
                                status = EventStatus.InProgress;
                                break;
                            case "завершено":
                            case "completed":
                                status = EventStatus.Completed;
                                break;
                            case "отменено":
                            case "cancelled":
                                status = EventStatus.Cancelled;
                                break;
                        }
                    }

                    // Проверяем на дублирование
                    if (events.Any(e => e.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    // Добавляем мероприятие
                    events.Add(new Event
                    {
                        Id = Event.GetNextId(events),
                        Title = title,
                        StartDate = startDate,
                        EndDate = endDate,
                        Status = status
                    });
                }
            }
        }



        public void SaveToExcel(string filePath, List<Volunteer> volunteers, List<Event> events)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Регистрации");

                // Заголовки
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("ID Волонтёра");
                header.CreateCell(1).SetCellValue("ФИО Волонтёра");
                header.CreateCell(2).SetCellValue("ID Мероприятия");
                header.CreateCell(3).SetCellValue("Название Мероприятия");
                header.CreateCell(4).SetCellValue("Статус Мероприятия");
                header.CreateCell(5).SetCellValue("Дата начала");

                // Данные
                for (int i = 0; i < _registrations.Count; i++)
                {
                    var reg = _registrations[i];
                    var volunteer = volunteers.FirstOrDefault(v => v.Id == reg.VolunteerId);
                    var ev = events.FirstOrDefault(e => e.Id == reg.EventId);

                    var row = sheet.CreateRow(i + 1);
                    row.CreateCell(0).SetCellValue(reg.VolunteerId);
                    row.CreateCell(1).SetCellValue(volunteer?.FullName ?? "");
                    row.CreateCell(2).SetCellValue(reg.EventId);
                    row.CreateCell(3).SetCellValue(ev?.Title ?? "");
                    row.CreateCell(4).SetCellValue(ev?.Status.ToString() ?? "");
                    row.CreateCell(5).SetCellValue(ev?.StartDate.ToString("dd.MM.yyyy") ?? "");
                }

                // Авторазмер столбцов
                for (int i = 0; i < 6; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                workbook.Write(fs);
            }
        }

        public void ExportAllRegistrations(string filePath, List<Volunteer> volunteers, List<Event> events)
        {
            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Все регистрации");

                // Заголовки
                var header = sheet.CreateRow(0);
                header.CreateCell(0).SetCellValue("ID Волонтёра");
                header.CreateCell(1).SetCellValue("Фамилия");
                header.CreateCell(2).SetCellValue("Имя");
                header.CreateCell(3).SetCellValue("Отчество");
                header.CreateCell(4).SetCellValue("Статус");
                header.CreateCell(5).SetCellValue("ID Мероприятия");
                header.CreateCell(6).SetCellValue("Название Мероприятия");
                header.CreateCell(7).SetCellValue("Статус Мероприятия");

                // Данные
                for (int i = 0; i < _registrations.Count; i++)
                {
                    var reg = _registrations[i];
                    var volunteer = volunteers.FirstOrDefault(v => v.Id == reg.VolunteerId);
                    var ev = events.FirstOrDefault(e => e.Id == reg.EventId);

                    var row = sheet.CreateRow(i + 1);
                    row.CreateCell(0).SetCellValue(reg.VolunteerId);
                    row.CreateCell(1).SetCellValue(volunteer?.LastName ?? "");
                    row.CreateCell(2).SetCellValue(volunteer?.FirstName ?? "");
                    row.CreateCell(3).SetCellValue(volunteer?.MiddleName ?? "");
                    row.CreateCell(4).SetCellValue(volunteer?.Status.ToString() ?? "");
                    row.CreateCell(5).SetCellValue(reg.EventId);
                    row.CreateCell(6).SetCellValue(ev?.Title ?? "");
                    row.CreateCell(7).SetCellValue(ev?.Status.ToString() ?? "");
                }

                // Авторазмер столбцов
                for (int i = 0; i < 8; i++)
                {
                    sheet.AutoSizeColumn(i);
                }

                workbook.Write(fs);
            }
        }
    }
}
