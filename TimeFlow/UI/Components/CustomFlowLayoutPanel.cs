using System.Windows.Forms;

namespace TimeFlow.UI.Components
{
    /// <summary>
    /// FlowLayoutPanel with double buffering enabled for smoother rendering
    /// </summary>
    public class CustomFlowLayoutPanel : FlowLayoutPanel
    {
        public CustomFlowLayoutPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | 
                         ControlStyles.AllPaintingInWmPaint | 
                         ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
}
