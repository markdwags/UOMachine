﻿using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using UOMachine.Resources;

namespace UOMachine
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    internal partial class OptionsWindow : Window
    {
        private bool myCancelEnabled = true;
        public bool CancelEnabled
        {
            get { return Utility.ThreadHelper.VolatileRead<bool>(ref myCancelEnabled); }
            internal set { myCancelEnabled = value; }
        }

        private enum BrowserType : byte
        {
            UOFolder,
            UOClient,
            Razor,
            UOS
        }
        internal delegate void dOptionsChanged(OptionsData optionsData);
        internal delegate void dOptionsCancelled();
        internal static event dOptionsChanged OptionsChangedEvent;
        internal static event dOptionsCancelled OptionsCancelledEvent;

        public OptionsWindow()
        {
            InitializeComponent();
        }

        public void LoadOptions(OptionsData options)
        {
            switch (options.CacheLevel)
            {
                case 0:
                    radioButtonNone.IsChecked = true;
                    radioButtonIndices.IsChecked = false;
                    radioButtonFull.IsChecked = false;
                    break;
                case 1:
                    radioButtonNone.IsChecked = false;
                    radioButtonIndices.IsChecked = true;
                    radioButtonFull.IsChecked = false;
                    break;
                case 2:
                    radioButtonNone.IsChecked = false;
                    radioButtonIndices.IsChecked = false;
                    radioButtonFull.IsChecked = true;
                    break;

            }
            textBoxUO.Text = options.UOFolder;
            textBoxRazor.Text = options.RazorFolder;
            textBoxUOS.Text = options.UOSFolder;
            textBoxUOSExe.Text = options.UOSExePath;
            textBoxSize.Text = options.TextEditorOptions.IndentationSize.ToString();
            textBoxServer.Text = options.Server;
            textBoxPort.Text = options.Port.ToString();
            textBoxClient.Text = options.UOClientPath;

            checkBoxEncryptedServer.IsChecked = options.EncryptedServer;
            checkBoxPatchClient.IsChecked = options.PatchClientEncryption;
            checkBoxEncryption.IsChecked = options.PatchClientEncryptionUOM;
            checkBoxStamina.IsChecked = options.PatchStaminaCheck;
            checkBoxLight.IsChecked = options.PatchAlwaysLight;
            checkBoxGameSize.IsChecked = options.PatchGameSize;
            if (options.PatchGameSizeWidth < 800)
                options.PatchGameSizeWidth = 800;
            if (options.PatchGameSizeHeight < 600)
                options.PatchGameSizeHeight = 600;
            gameWidth.Text = options.PatchGameSizeWidth.ToString();
            gameHeight.Text = options.PatchGameSizeHeight.ToString();

            if (options.TextEditorOptions.ConvertTabsToSpaces)
                checkBoxConvert.IsChecked = true;
            else checkBoxConvert.IsChecked = false;
            if (options.TextEditorOptions.CutCopyWholeLine)
                checkBoxCopy.IsChecked = true;
            else checkBoxCopy.IsChecked = false;
            if (options.TextEditorOptions.ShowBoxForControlCharacters)
                checkBoxControl.IsChecked = true;
            else checkBoxControl.IsChecked = false;
            if (options.TextEditorOptions.ShowSpaces)
                checkBoxSpace.IsChecked = true;
            else checkBoxSpace.IsChecked = false;
            if (options.TextEditorOptions.ShowTabs)
                checkBoxTab.IsChecked = true;
            else checkBoxTab.IsChecked = false;

        }

        private static void OnOptionsChanged(OptionsData optionsData)
        {
            dOptionsChanged handler = OptionsChangedEvent;
            if (handler != null) handler(optionsData);
        }

        private static void OnCancel()
        {
            dOptionsCancelled handler = OptionsCancelledEvent;
            if (handler != null) handler();
        }

        private bool SaveOptions()
        {
            OptionsData od = new OptionsData();
            ushort port;
            if (ushort.TryParse(textBoxPort.Text, out port))
                od.Port = port;
            else
            {
                System.Windows.MessageBox.Show( Strings.Invalidvalueforport, Strings.Error );
                textBoxPort.Text = "";
                return false;
            }
            od.Server = textBoxServer.Text;
            if ((bool)radioButtonNone.IsChecked) od.CacheLevel = 0;
            else if ((bool)radioButtonIndices.IsChecked) od.CacheLevel = 1;
            else if ((bool)radioButtonFull.IsChecked) od.CacheLevel = 2;
            od.PatchClientEncryption = (bool)checkBoxPatchClient.IsChecked;
            od.EncryptedServer = (bool)checkBoxEncryptedServer.IsChecked;
            od.PatchClientEncryptionUOM = (bool)checkBoxEncryption.IsChecked;
            od.PatchStaminaCheck = (bool)checkBoxStamina.IsChecked;
            od.PatchAlwaysLight = (bool)checkBoxLight.IsChecked;
            od.PatchGameSize = (bool)checkBoxGameSize.IsChecked;
            od.PatchGameSizeWidth = int.Parse(gameWidth.Text);
            od.PatchGameSizeHeight = int.Parse(gameHeight.Text);
            if (od.PatchGameSizeWidth < 800)
                od.PatchGameSizeWidth = 800;
            if (od.PatchGameSizeHeight < 600)
                od.PatchGameSizeHeight = 600;
            od.UOFolder = textBoxUO.Text;
            od.UOClientPath = textBoxClient.Text;
            od.RazorFolder = textBoxRazor.Text;
            od.UOSFolder = textBoxUOS.Text;
            od.UOSExePath = textBoxUOSExe.Text;
            od.TextEditorOptions.ConvertTabsToSpaces = (bool)checkBoxConvert.IsChecked;
            od.TextEditorOptions.CutCopyWholeLine = (bool)checkBoxCopy.IsChecked;
            od.TextEditorOptions.ShowBoxForControlCharacters = (bool)checkBoxControl.IsChecked;
            od.TextEditorOptions.ShowSpaces = (bool)checkBoxSpace.IsChecked;
            od.TextEditorOptions.ShowTabs = (bool)checkBoxTab.IsChecked;

            int tabSize;
            try { tabSize = Convert.ToInt32(textBoxSize.Text); }
            catch { tabSize = od.TextEditorOptions.IndentationSize; }
            od.TextEditorOptions.IndentationSize = tabSize;

            if (!od.IsValid())
            {
                System.Windows.MessageBox.Show( Strings.Invalidvaluepresentinoptions, Strings.Error );
                return false;
            }
            OnOptionsChanged(od);
            OptionsData.Serialize("options.xml", od);
            return true;
        }

        private void ShowFolderBrowser(string path, BrowserType type)
        {
            FolderBrowserDialog fdg = new FolderBrowserDialog();
            fdg.RootFolder = Environment.SpecialFolder.MyComputer;
            DialogResult d = fdg.ShowDialog();
            if (d == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(fdg.SelectedPath))
            {
                if (type == BrowserType.UOFolder) textBoxUO.Text = fdg.SelectedPath;
                else if (type == BrowserType.Razor) textBoxRazor.Text = fdg.SelectedPath;
                else if (type == BrowserType.UOS) textBoxUOS.Text = fdg.SelectedPath;
            }
        }

        private void buttonUO_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowser(textBoxUO.Text, BrowserType.UOFolder);
        }

        private void buttonRazor_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowser(textBoxRazor.Text, BrowserType.Razor);
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            OnCancel();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            if (SaveOptions()) this.Hide();
        }

        private void buttonClient_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
            ofd.ValidateNames = true;
            string path = "";
            if (!string.IsNullOrEmpty(textBoxClient.Text))
            {
                try
                {
                    path = System.IO.Path.GetDirectoryName(textBoxClient.Text);
                }
                catch { }
            }
            ofd.InitialDirectory = path;
            DialogResult d = ofd.ShowDialog();
            if (d == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(ofd.FileName))
            {
                textBoxClient.Text = ofd.FileName;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (this.CancelEnabled)
            {
                e.Cancel = true;
                this.Hide();
            }
            OnCancel();
        }

        private void buttonUOS_Click(object sender, RoutedEventArgs e)
        {
            ShowFolderBrowser(textBoxUOS.Text, BrowserType.UOS);
        }

        private void buttonUOSExe_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckFileExists = true;
            ofd.CheckPathExists = true;
            ofd.Multiselect = false;
            ofd.Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*";
            ofd.ValidateNames = true;
            string path = "";
            if (!string.IsNullOrEmpty(textBoxUOS.Text))
            {
                try
                {
                    path = System.IO.Path.GetDirectoryName(textBoxUOS.Text);
                }
                catch { }
            }
            ofd.InitialDirectory = path;
            DialogResult d = ofd.ShowDialog();
            if (d == System.Windows.Forms.DialogResult.OK && !string.IsNullOrEmpty(ofd.FileName))
            {
                textBoxUOSExe.Text = ofd.FileName;
            }
        }

        private void textBoxRazor_TextChanged( object sender, TextChangedEventArgs e )
        {

        }
    }
}
