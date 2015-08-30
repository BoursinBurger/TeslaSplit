using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TeslaSplit
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }


        public static void PrependLine(this StringBuilder sb, string value)
        {
            sb.Insert(0, value + Environment.NewLine);
        }
    }

    public static class ControlsDelegate
    {
        private delegate void SetTextCallback(Form f, Control ctrl, string text);

        public static void SetText(Form form, Control ctrl, string text)
        {
            if (ctrl.InvokeRequired)
            {
                SetTextCallback stc = SetText;
                form.Invoke(stc, new object[] {form, ctrl, text});
            }
            else
            {
                ctrl.Text = text;
            }
        }
    }
}
