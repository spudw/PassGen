using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace PassGen
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //MessageBox.Show("Application Starting\nArgument Count: " + e.Args.Length);
        }
    }

    public partial class MainGUI : Window
    {
        private System.Windows.Forms.NotifyIcon NotifyIcon;
        Window WinKeyMgr = new KeyManager();
        const string sVersion = "0.1.20210903";
        const string sTitle = "PassGen v" + sVersion;
        //const string sTitle = "PassGen";
        const string sEmptyPasshrase = "Type Passphrase Here";
        const string sPassphraseIsTooShort = "Passphrase is too short.\r\nMust contain at least 8 characters.";
        const string sCopiedToClipboard = "Copied to clipboard.";
        DispatcherTimer aTimer = new DispatcherTimer();
        const long iAutoPurgeTimerValue = 1;  //minutes

        public MainGUI()
        {
            InitializeComponent();
            PassGen.Title = sTitle;
            aTimer.Tick += AutoPurge;
            PassGen_NotifyIconInit();
            PassGen_CloseToTrayInit();
            PassGen_LoadCurrentKey();
            if (PasswdKey_Read()!=null) { Passphrase_SetStyle(1); }
        }

        #region GUI Event Logic
        private void PassGen_Activated(object sender, EventArgs e)
        {
            if (WinKeyMgr.IsVisible) 
            {
                WinKeyMgr.Activate();
            }
            else
            {
                PassGen_LoadCurrentKey();
            }
        }

        private void PassGen_Closed(object sender, EventArgs e)
        {
            if (!(WinKeyMgr == null)) { WinKeyMgr.Close(); }
            NotifyIcon.Dispose();
            AutoPurge(null, null);
            System.Environment.Exit(0);
        }

        private void PassGen_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MnuOptionsCloseToTray.IsChecked == true)
            {
                e.Cancel = true;
                PassGen_MinimizeToTray();
            }
        }

        private void PassGen_Deactivated(object sender, EventArgs e)
        {
            BtnReveal_Checked(false);
            MaskKey();
            MaskPassword();
        }
        #endregion

        #region Control Fundamental Logic
        private void BtnKeyMgr_Enable(bool bFlag = true)
        {
            BtnKeyMgr.IsEnabled = bFlag;
        }

        private void BtnPassphraseCopy_Enable(bool bFlag = true)
        {
            BtnPassphraseCopy.IsEnabled = bFlag;
        }

        private void BtnPasswordCopy_Enable(bool bFlag = true)
        {
            BtnPasswordCopy.IsEnabled = bFlag;
        }

        private void BtnReveal_Checked(bool bFlag = true)
        {
            BtnReveal.IsChecked = bFlag;
            BtnReveal_SetText((bFlag == true) ? "H_ide" : "_Show");
        }

        private void BtnReveal_Enable(bool bFlag = true)
        {
            BtnReveal.IsEnabled = bFlag;
        }

        private void BtnReveal_SetText(string sText)
        {
            BtnReveal.Content = sText;
        }

        private bool BtnReveal_GetCheckedState()
        {
            return BtnReveal.IsChecked.Value;
        }

        private void LblPassphraseMsg_SetText(string sText)
        {
            LblPassphraseMsg.Content = sText;
        }

        private void LblPasswordMsg_SetText(string sText)
        {
            LblPasswordMsg.Content = sText;
        }

        private string PasswdKey_Read()
        {
            return PasswdKey.Password;
        }

        private void PasswdKey_Set(string sText)
        {
            PasswdKey.Password = sText;
        }

        private void PasswdKey_Visible(bool bFlag = true)
        {
            PasswdKey.Visibility = (bFlag == true) ? Visibility.Visible : Visibility.Hidden;
        }

        private void PasswdPassword_Enable(bool bFlag = true)
        {
            PasswdPassword.IsEnabled = bFlag;
            if (bFlag == true)
            {
                PasswdPassword.Background = Brushes.White;
            }
            else
            {
                BrushConverter bc = new BrushConverter();
                Brush brush = (Brush)bc.ConvertFrom("#F0F0F0");
                brush.Freeze();
                PasswdPassword.Background = brush;
            }
        }

        private string PasswdPassword_Read()
        {
            return PasswdPassword.Password;
        }

        private void PasswdPassword_Set(string sText)
        {
            PasswdPassword.Password = sText;
        }

        private void PasswdPassword_Visible(bool bFlag)
        {
            PasswdPassword.Visibility = (bFlag == true) ? Visibility.Visible : Visibility.Hidden;
        }
        private string TxtKey_Read()
        {
            return TxtKey.Text;
        }

        private void TxtKey_Set(string sValue)
        {
            TxtKey.Text = sValue;
        }

        private void TxtKey_Visible(bool bFlag = true)
        {
            TxtKey.Visibility = (bFlag == true) ? Visibility.Visible : Visibility.Hidden;
        }

        private void TxtPassphrase_Enable(bool bFlag = true)
        {
            TxtPassphrase.IsEnabled = bFlag;
            if (bFlag == true)
            {
                TxtPassphrase.Background = Brushes.White;
            }
            else
            {
                BrushConverter bc = new BrushConverter();
                Brush brush = (Brush)bc.ConvertFrom("#F0F0F0");
                brush.Freeze();
                TxtPassphrase.Background = brush;
            }
        }

        private string TxtPassphrase_Read()
        {
            //if (TxtPassphrase.Text.Length > 0)
            //{
                return TxtPassphrase.Text.Trim();
            //}
            //return null;
        }

        private string TxtPassword_Read()
        {
            return TxtPassword.Text;
        }

        private void TxtPassword_Set(string sText)
        {
            TxtPassword.Text = sText;
        }

        private void TxtPassword_Visible(bool bFlag = true)
        {
            TxtPassword.Visibility = (bFlag == true) ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion

        #region Control Event Logic
        private void BtnKeyMgr_Click(object sender, RoutedEventArgs e)
        {
            OpenKeyManager();
        }
        
        private void BtnPassphraseCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyPassphrase();
        }

        private void BtnPasswordCopy_Click(object sender, RoutedEventArgs e)
        {
            CopyPassword();
        }

        private void BtnReveal_Click(object sender, RoutedEventArgs e)
        {
            bool bBtnState = BtnReveal_GetCheckedState();
            BtnReveal_Checked(bBtnState);
            MaskKey(!bBtnState);
        }
        
        private void MnuFileKeyMgr_Click(object sender, RoutedEventArgs e)
        {
            OpenKeyManager();
        }

        private void MnuFileQuit_Click(object sender, RoutedEventArgs e)
        {
            CloseApp();
        }

        private void MnuOptionsAutoStart_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MnuOptionsCloseToTray_Click(object sender, RoutedEventArgs e)
        {
            GUICloseToTrayDisable(!MnuOptionsCloseToTray.IsChecked);
        }

        private void TxtPassphrase_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtPassphrase.Text == sEmptyPasshrase)
            {
                Passphrase_SetStyle(0);
            }
        }

        private void TxtPassphrase_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TxtPassphrase.Text == "")
            {
                Passphrase_SetStyle(1);
            }
        }

        private void TxtPassphrase_TextChanged(object sender, TextChangedEventArgs e)
        {
            string sPassphraseText = TxtPassphrase_Read();
            int iPassphraseTextLen = sPassphraseText.Length;
            if (iPassphraseTextLen >= 8 && sPassphraseText != sEmptyPasshrase && sPassphraseText != "")
            {
                PassGen_GeneratePassword();
            }
            else if (iPassphraseTextLen < 8)
            {
                LblPassphraseMsg_SetText(null);
                PasswdPassword_Enable(false);
                PasswdPassword_Set(null);
                TxtPassword_Set(null);
                LblPasswordMsg_SetText(null);
                BtnPassphraseCopy_Enable(false);
                BtnPasswordCopy_Enable(false);
                if (TxtPassphrase.Text.Length > 0)
                {
                    LblPassphraseMsg_SetText(sPassphraseIsTooShort);
                }
                else
                {
                    LblPassphraseMsg_SetText(null);
                }
            }
        }

        private void TxtPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            MaskPassword();
        }

        private void PasswdPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            MaskPassword(false);
            TxtPassword.Focus();
        }
        #endregion

        #region Additional UI Logic
        private void AutoPurge(object sender, EventArgs e)
        {
            aTimer.IsEnabled = false;
            aTimer.Stop();

            Console.WriteLine("...");
            string sClipboard = PassGenFuncs.ClipboardGetText();
            if (sClipboard == PasswdPassword_Read() || sClipboard == TxtPassphrase_Read())
            {
                PassGenFuncs.ClipboardClear();
            }
            LblPassphraseMsg_SetText(null);
            LblPasswordMsg_SetText(null);
        }
        
        private void CopyPassphrase()
        {
            string sPassphrase = TxtPassphrase_Read();
            if ((sPassphrase != sEmptyPasshrase && TxtPassword_Read().Length != 0) == false) { return; }
            PassGenFuncs.ClipboardSetText(sPassphrase);
            LblPasswordMsg_SetText(null);
            LblPassphraseMsg_SetText(sCopiedToClipboard);
        }

        private void CopyPassword()
        {
            string sPassword = TxtPassword_Read();
            if (sPassword.Length == 0) { return; }
            PassGenFuncs.ClipboardSetText(sPassword);
            LblPassphraseMsg_SetText(null);
            LblPasswordMsg_SetText(sCopiedToClipboard);
        }

        private void GUICloseToTrayDisable(bool bFlag = true)
        {
            RegistryKey oKey = PassGenFuncs.RegistryOpen(true);
            if (bFlag == true)
            {
                oKey.SetValue("NoCloseToTray", 1);
            }
            else if (bFlag == false)
            {
                oKey.DeleteValue("NoCloseToTray");
            }
            oKey.Close();
        }

        private bool GUICloseToTrayIsDisabled()
        {
            RegistryKey oKey = PassGenFuncs.RegistryOpen();
            var Value = oKey.GetValue("NoCloseToTray");
            oKey.Close();
            return (Value == null) ? false : true;
        }

        private void MaskPassword(bool bFlag = true)
        {
            TxtPassword_Visible(!bFlag);
            PasswdPassword_Visible(bFlag);
        }

        private void MaskKey(bool bFlag = true)
        {
            PasswdKey_Visible(bFlag);
            TxtKey_Visible(!bFlag);
        }

        private void NotifyIcon_Click(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this.Show();
                this.ShowInTaskbar = true;
                this.WindowState = WindowState.Normal;
                this.Activate();
                NotifyIcon.Visible = false;
            }
        }

        private void NotifyIcon_ContextMenuOpen(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            this.WindowState = WindowState.Normal;
            this.Activate();
            NotifyIcon.Visible = false;
        }

        private void NotifyIcon_ContextMenuQuit(object sender, EventArgs e)
        {
            MnuOptionsCloseToTray.IsChecked = false;
            this.Close();
        }

        private void OpenKeyManager()
        {
            if (!(WinKeyMgr.IsVisible))
            {
                //WinKeyMgr.Show();
                WinKeyMgr.ShowDialog();
            }
        }

        private void PassGen_CloseToTrayInit()
        {
            if (GUICloseToTrayIsDisabled()) { MnuOptionsCloseToTray.IsChecked = false; }
        }

        public void PassGen_LoadCurrentKey()
        {
            string CurrentKey = PassGenFuncs.KeyReadCurrent();
            if (CurrentKey != null)
            {
                TxtKey_Set(CurrentKey);
                PasswdKey_Set(CurrentKey);
                BtnReveal_Enable();
                //Passphrase_SetStyle(1);
                TxtPassphrase_Enable();
                PassGen_GeneratePassword();
            }
            else
            {
                TxtKey_Set(null);
                PasswdKey_Set(null);
                BtnReveal_Enable(false);
                //Passphrase_SetStyle(0);
                TxtPassphrase_Enable(false);
            }
        }

        private void PassGen_NotifyIconInit()
        {
            NotifyIcon = new System.Windows.Forms.NotifyIcon();
            NotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetExecutingAssembly().Location);
            NotifyIcon.MouseClick += new System.Windows.Forms.MouseEventHandler(NotifyIcon_Click);
            NotifyIcon.Text = "PassGen";
            NotifyIcon.BalloonTipTitle = "PassGen";
            NotifyIcon.BalloonTipText = "PassGen has moved to the Notification Tray.";
            NotifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            NotifyIcon.ContextMenuStrip.Items.Add(sTitle).Enabled = false;
            NotifyIcon.ContextMenuStrip.Items.Add("-");
            var Item = NotifyIcon.ContextMenuStrip.Items.Add("Open", null, this.NotifyIcon_ContextMenuOpen);
            Item.Font = new System.Drawing.Font(Item.Font, Item.Font.Style | System.Drawing.FontStyle.Bold);
            NotifyIcon.ContextMenuStrip.Items.Add("Quit", null, this.NotifyIcon_ContextMenuQuit);
        }

        private void Passphrase_SetStyle(int iStyle = 0)
        {
            if (iStyle < 0 || iStyle > 1) { return; }

            LblPassphraseMsg_SetText(null);
            LblPasswordMsg_SetText(null);
            
            if (iStyle == 1)
            {
                TxtPassphrase.Text = sEmptyPasshrase;
                TxtPassphrase.TextAlignment = TextAlignment.Center;
                TxtPassphrase.FontStyle = FontStyles.Italic;
                TxtPassphrase.Foreground = Brushes.LightGray;
                PasswdPassword.Password = "";
                TxtPassword.Text = PasswdPassword.Password;
            }
            else if (iStyle == 0)
            {
                TxtPassphrase.Text = "";
                TxtPassphrase.TextAlignment = TextAlignment.Left;
                TxtPassphrase.FontStyle = FontStyles.Normal;
                TxtPassphrase.Foreground = Brushes.Black;
                TxtPassphrase.Background = Brushes.White;
            }
        }
        #endregion

        private void MnuFileKeyMgr_Command(object sender, EventArgs e)
        {
            //if (MnuFileKeyMgr.IsEnabled == false) { return; }
            OpenKeyManager();
        }

        private void MnuFileQuit_Command(object sender, EventArgs e)
        {
            CloseApp();
        }
        private void CloseApp()
        {
            MnuOptionsCloseToTray.IsChecked = false;
            this.Close();
        }

        private void MainGUI_EscCommand(object sender, RoutedEventArgs e)
        {
            if (MnuOptionsCloseToTray.IsChecked == true)
            {
                PassGen_MinimizeToTray();
            }
        }

        private void PassGen_MinimizeToTray()
        {
            this.WindowState = WindowState.Minimized;
            this.Hide();
            PassGen.ShowInTaskbar = false;
            NotifyIcon.Visible = true;
            NotifyIcon.ShowBalloonTip(3500);
        }

        private void PassGen_GeneratePassword()
        {
            string KeyText = PasswdKey_Read();
            string PassphraseText = TxtPassphrase_Read();
            if (KeyText == null) { return; }
            if (PassphraseText.Length < 8) { return; }
            if (PassphraseText == sEmptyPasshrase || PassphraseText == "") { return; }
            string Password = PassGenFuncs.GeneratePassword(KeyText, PassphraseText);
            if (PasswdPassword_Read() == Password) { return; }
            PasswdPassword_Set(Password);
            TxtPassword_Set(Password);
            PasswdPassword_Enable();
            BtnPassphraseCopy_Enable();
            BtnPasswordCopy_Enable();
            CopyPassword();
            aTimer.IsEnabled = false;
            aTimer.Stop();
            aTimer.Interval = TimeSpan.FromMinutes(iAutoPurgeTimerValue);
            aTimer.Start();
            aTimer.IsEnabled = true;
        }
    }
}