using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BT3_LTMCB
{
    public class MainTaskInterface : Form
    {
        private Panel scrollPanel;
        private TableLayoutPanel calendarGrid;
        private const int TotalDays = 7;
        private const int StartHour = 0;
        private const int EndHour = 24;
        private const int IntervalsPerHour = 2;
        private const float RowHeight = 30f;
        private const float HeaderHeight = 50f;

        private string[] dayNames = { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };

        public class TaskEvent
        {
            public string Title { get; set; }
            public DayOfWeek Day { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public Color TaskColor { get; set; }
        }

        public MainTaskInterface()
        {
            this.WindowState = FormWindowState.Maximized;

            InitializeCalendarGrid();
            LoadSampleTasks();
        }

        private void InitializeCalendarGrid()
        {
            scrollPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            this.Controls.Add(scrollPanel);

            int totalIntervals = (EndHour - StartHour) * IntervalsPerHour;

            int totalGridHeight = (int)(HeaderHeight + (totalIntervals * RowHeight));

            calendarGrid = new TableLayoutPanel
            {
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = totalGridHeight,

                // === SỬA LỖI: XÓA DÒNG ANCHOR BÊN DƯỚI ===
                // Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right 
            };
            scrollPanel.Controls.Add(calendarGrid);

            calendarGrid.ColumnCount = 1 + TotalDays;
            calendarGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            float percentPerDay = 100f / TotalDays;
            for (int i = 0; i < TotalDays; i++)
            {
                calendarGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, percentPerDay));
            }

            calendarGrid.RowCount = 1 + totalIntervals;

            calendarGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, HeaderHeight));

            for (int i = 0; i < totalIntervals; i++)
            {
                calendarGrid.RowStyles.Add(new RowStyle(SizeType.Absolute, RowHeight));
            }

            for (int i = 0; i < TotalDays; i++)
            {
                Label dayLabel = new Label
                {
                    Text = dayNames[i],
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Font = new Font(this.Font, FontStyle.Bold)
                };
                calendarGrid.Controls.Add(dayLabel, i + 1, 0);
            }

            int currentRow = 1;
            for (int hour = StartHour; hour < EndHour; hour++)
            {
                Label hourLabel = new Label
                {
                    Text = $"{hour:D2}:00",
                    TextAlign = ContentAlignment.TopRight,
                    Dock = DockStyle.Fill
                };
                calendarGrid.Controls.Add(hourLabel, 0, currentRow);
                calendarGrid.SetRowSpan(hourLabel, IntervalsPerHour);
                currentRow += IntervalsPerHour;
            }
        }

        private void LoadSampleTasks()
        {
            List<TaskEvent> tasks = new List<TaskEvent>
            {
                new TaskEvent { Title = "SS006.Q16\nP. B7.02", Day = DayOfWeek.Monday, StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(9, 0, 0), TaskColor = Color.CornflowerBlue },
                new TaskEvent { Title = "SS003.Q14\nP. B3.14", Day = DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 30, 0), TaskColor = Color.MediumPurple },
                new TaskEvent { Title = "SS010.Q16\nP. B3.14", Day = DayOfWeek.Monday, StartTime = new TimeSpan(14, 30, 0), EndTime = new TimeSpan(16, 15, 0), TaskColor = Color.DarkGray },

                new TaskEvent { Title = "IT002.012\nP. B5.10", Day = DayOfWeek.Tuesday, StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(9, 45, 0), TaskColor = Color.MediumSeaGreen },
                new TaskEvent { Title = "IT002.Q12.2\nP. B2.10", Day = DayOfWeek.Tuesday, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 0, 0), TaskColor = Color.MediumSeaGreen },

                new TaskEvent { Title = "NT106.Q12\nP. E10.1", Day = DayOfWeek.Wednesday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 30, 0), TaskColor = Color.Coral },
                new TaskEvent { Title = "NT106.Q12.2\nP. B3.02", Day = DayOfWeek.Wednesday, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(17, 0, 0), TaskColor = Color.Coral },

                new TaskEvent { Title = "IT004.Q16.2\nP. B2.18", Day = DayOfWeek.Thursday, StartTime = new TimeSpan(7, 30, 0), EndTime = new TimeSpan(11, 30, 0), TaskColor = Color.LightCoral }
            };

            foreach (var task in tasks)
            {
                AddTaskToCalendar(task);
            }
        }

        private void AddTaskToCalendar(TaskEvent task)
        {
            int column = -1;
            switch (task.Day)
            {
                case DayOfWeek.Monday: column = 1; break;
                case DayOfWeek.Tuesday: column = 2; break;
                case DayOfWeek.Wednesday: column = 3; break;
                case DayOfWeek.Thursday: column = 4; break;
                case DayOfWeek.Friday: column = 5; break;
                case DayOfWeek.Saturday: column = 6; break;
                case DayOfWeek.Sunday: column = 7; break;
            }
            if (column == -1) return;

            double totalMinutesStart = task.StartTime.TotalMinutes;
            int startRow = (int)Math.Floor(totalMinutesStart / 30.0) + 1;

            TimeSpan duration = task.EndTime - task.StartTime;
            double totalSlots = duration.TotalMinutes / 30.0;
            int rowSpan = (int)Math.Round(totalSlots);
            if (rowSpan <= 0) rowSpan = 1;

            Panel taskPanel = new Panel
            {
                BackColor = task.TaskColor,
                Margin = new Padding(2),
                Dock = DockStyle.Fill
            };

            Label taskLabel = new Label
            {
                Text = task.Title,
                ForeColor = Color.Black,
                Dock = DockStyle.Fill,
                Padding = new Padding(3),
                Font = new Font(this.Font.FontFamily, 8f)
            };
            taskPanel.Controls.Add(taskLabel);

            calendarGrid.Controls.Add(taskPanel, column, startRow);
            calendarGrid.SetRowSpan(taskPanel, rowSpan);
        }
    }
}