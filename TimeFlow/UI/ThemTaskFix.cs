using System.Windows.Forms;

namespace TimeFlow.UI
{
    public partial class FormThemTask : Form
    {
        
        private int? _taskIdToEdit = null;
        public FormThemTask(int taskId) : this()
        {
            _taskIdToEdit = taskId;

            this.Text = "Chỉnh sửa Task";
        }
    }
}