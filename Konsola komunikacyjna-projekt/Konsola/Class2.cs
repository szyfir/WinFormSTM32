using System;
using System.Windows.Forms;

public class VerticalProgressBar : ProgressBar
{
    protected override CreateParams CreateParams
    {
        get
        {
          ForeColor = System.Drawing.Color.Red;
            CreateParams cp = base.CreateParams;
            cp.Style |= 0x04;
            return cp;
        }
    }

    private void InitializeComponent()
    {
            this.SuspendLayout();
            // 
            // VerticalProgressBar
            // 
            this.ForeColor = System.Drawing.Color.Red;
            this.ResumeLayout(false);

    }
}