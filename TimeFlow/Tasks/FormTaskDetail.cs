using System;
using System.Drawing;
using System.Windows.Forms;
using TimeFlow.UI.Components;
using TimeFlow.Models;

namespace TimeFlow.Tasks
{
    public partial class FormTaskDetail : Form
    {
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;

        private TaskModel _currentTask;

        // Constructor mặc định (fallback)
        public FormTaskDetail()
        {
            // Sử dụng task đầu tiên làm demo nếu không pass task
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        // Constructor nhận task data
        public FormTaskDetail(TaskModel task) : this()
        {
            _currentTask = task ?? throw new ArgumentNullException(nameof(task));
        }

        private TimeFlow.UI.Components.CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height, Color? hoverColor)
        {
            return new TimeFlow.UI.Components.CustomButton
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                HoverColor = hoverColor ?? AppColors.Blue600,
                BorderRadius = 8,
                Width = width,
                Height = height,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 12)
            };
        }

        private TimeFlow.UI.Components.CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height)
        {
            return CreateMenuButton(text, backColor, foreColor, width, height, null);
        }

        private Control CreateComment(string user, string text, string time)
        {
            FlowLayoutPanel comment = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false, 
                Width = 800,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(0, 0, 0, 8),
                BackColor = AppColors.Gray50,
                BorderStyle = BorderStyle.FixedSingle
            };
            comment.AutoSize = true;
            comment.WrapContents = false;

            FlowLayoutPanel header = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            header.Controls.Add(new Label { Text = user, Font = FontBold, ForeColor = AppColors.Gray800, AutoSize = true, Margin = new Padding(0, 0, 8, 0) });
            header.Controls.Add(new Label { Text = time, Font = FontRegular, ForeColor = AppColors.Gray500, AutoSize = true });
            comment.Controls.Add(header);

            Label content = new Label
            {
                Text = text,
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                MaximumSize = new Size(comment.Width, 0),
                AutoSize = true
            };
            comment.Controls.Add(content);

            return comment;
        }

        private Control CreateActivityLog(string activity, string time, int width)
        {
            FlowLayoutPanel logItem = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                Width = width,
                Margin = new Padding(0, 0, 0, 12)
            };
            logItem.AutoSize = true;

            Label lblActivity = new Label
            {
                Text = activity,
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                MaximumSize = new Size(width, 0),
                AutoSize = true
            };
            Label lblTime = new Label
            {
                Text = time,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = AppColors.Gray400,
                AutoSize = true
            };

            logItem.Controls.Add(lblActivity);
            logItem.Controls.Add(lblTime);
            return logItem;
        }
    }
}