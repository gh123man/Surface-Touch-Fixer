using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SurfaceTouchFixer {
    class SurfaceTouchFixer : Form {

        public static class UnsafeNativeMethods {
            [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
            public static extern bool SetForegroundWindow(HandleRef hWnd);
        }

        private const string RESOURCE_EXE = "SurfaceTouchFixer.Resources.DevManView.exe";
        private const string REAl_EXE = "DevManView.exe";
        private const string EXIT = "exit";
        private const string NAME = "SurfaceTouchFixer";
        private const string ARG = "/disable_enable \"HID-compliant touch screen\"";

        private NotifyIcon mTrayIcon;
        private ContextMenuStrip mMenuStrip;
        private string exePath;


        [STAThread]
        static void Main() {
            Application.Run(new SurfaceTouchFixer());
        }

        public SurfaceTouchFixer() {
            Stream stream = GetType().Assembly.GetManifestResourceStream(RESOURCE_EXE);
            byte[] bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            exePath = Path.GetTempPath() + REAl_EXE;
            File.WriteAllBytes(exePath, bytes);

            mMenuStrip = new ContextMenuStrip();
            ToolStripMenuItem menuItem = new ToolStripMenuItem(EXIT);
            menuItem.Click += new EventHandler(onExit);
            menuItem.Name = "Exit";
            mMenuStrip.Items.Add(menuItem);
            mMenuStrip.MouseLeave += this.closeMenu;


            mTrayIcon = new NotifyIcon();
            mTrayIcon.Text = NAME;
            mTrayIcon.Icon = new Icon(Properties.Resources.icon, SystemInformation.SmallIconSize);

            mTrayIcon.MouseClick += this.mouseDown;

            mTrayIcon.Visible = true;
        }

        public void restartDriver() {

            Process.Start(exePath, ARG);

        }

        public void closeMenu(object sender, EventArgs e) {
            mMenuStrip.Hide();
        }

        public void mouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            if (e.Button == System.Windows.Forms.MouseButtons.Right) {
                restartDriver();
            } else {
                if (mMenuStrip.Visible) {
                    mMenuStrip.Hide();
                } else {
                    //Sort of hack to remove the task bar button.
                    UnsafeNativeMethods.SetForegroundWindow(new HandleRef(mMenuStrip, mMenuStrip.Handle));
                    mMenuStrip.Show(Cursor.Position);
                }
            }
        }

        protected override void OnLoad(EventArgs e) {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void onExit(object sender, EventArgs e) {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing) {
            if (isDisposing) {
                mTrayIcon.Dispose();
            }
            File.Delete(exePath);
            base.Dispose(isDisposing);
        }

    }
}
