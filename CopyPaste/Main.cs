using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace CopyPaste
{
    public partial class Main : Form
    {

        List<int> keys = new List<int>();

        private static int key = 0;
 
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifers, int vlc);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);


        private const byte VK_CONTROL = 0x11;
        private const byte VK_V = 0x56;
        private const uint KEYEVENTF_KEYUP = 0x0002;

        public static void PasteToFocusedWindow(RichTextBox rtf)
        {
            //DataObject dobj = new DataObject();
            //dobj.SetData(DataFormats.Text, rtf.SelectedText);
            //dobj.SetData(DataFormats.Rtf, rtf.Rtf);
            //Clipboard.SetDataObject(dobj);

            Clipboard.SetText(rtf.Text);
            IntPtr focusedHandle = GetForegroundWindow();

            if (focusedHandle != IntPtr.Zero)
            {
                SetForegroundWindow(focusedHandle);

                keybd_event(VK_CONTROL, 0, 0, 0);
                keybd_event(VK_V, 0, 0, 0);
                keybd_event(VK_V, 0, KEYEVENTF_KEYUP, 0);
                keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, 0);
            }
        }

        public Main()
        {
            InitializeComponent();

        }
         
        private void MyTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox currentTextBox = sender as TextBox;
            if (currentTextBox != null)
            {
                currentTextBox.Clear();
                currentTextBox.Text = e.KeyCode.ToString();

                int key = Convert.ToInt32(System.Text.RegularExpressions.Regex.Replace(currentTextBox.Name, "[^0-9]+", string.Empty));
                UnregisterHotKey(this.Handle, key);

                if (!RegisterHotKey(this.Handle, key, 0, (int)e.KeyCode))
                {
                    MessageBox.Show("Failed to use this key.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                e.Handled = true; 
            }
        }
       
        private void Form1_Load(object sender, EventArgs e)
        {

        }

     
        protected override void WndProc(ref Message m)
        {

            if (m.Msg == 0x0312)
            {
                bool hasVal = keys.IndexOf(m.WParam.ToInt32()) != -1;
                if(hasVal)
                {
                    Panel panel = (Panel)Controls["panelCopy" + m.WParam.ToInt32().ToString()];

                    string textBoxName = "copyBox" + m.WParam.ToInt32().ToString();
                    RichTextBox textBox = (RichTextBox)panel.Controls[textBoxName];
                    if (textBox != null)
                        PasteToFocusedWindow(textBox);
                }
               
                      
            }
            base.WndProc(ref m);
        }

        private Panel CreatePanel(int key)
        {
            Panel pnl = new Panel
            {
                Name = "PanelCopy" + key,
                BorderStyle = BorderStyle.FixedSingle,
                Width = 302,
                Height = 103,
                Location = new Point(6, 41 + (key * 110))
            };

            Label lbl = new Label
            {
                Text = "Text",
                Location = new Point(5, 6)
            };

            Label lbl2 = new Label
            {
                Text = "Key",
                Location = new Point(226, 6)
            };

            RichTextBox textBox = new RichTextBox
            {
                Name = "copyBox" + key,
                Text = "Insert text",
                Width = 213,
                Height = 70,
                Location = new Point(8, 22)
            };

            TextBox keyBox = new TextBox
            {
                Name = "keyBox" + key,
                Width = 70,
                Height = 20,
                ReadOnly = true,
                Location = new Point(227, 22)
            };

            keyBox.KeyDown += new KeyEventHandler(MyTextBox_KeyDown);

            pnl.Controls.Add(textBox);
            pnl.Controls.Add(keyBox);
            pnl.Controls.Add(lbl);
            pnl.Controls.Add(lbl2);

            return pnl;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            Panel pnl = CreatePanel(key);
            this.Controls.Add(pnl);

            keys.Add(key);
            key++;
        }
    }
}
