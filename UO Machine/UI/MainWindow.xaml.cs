using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Windows.Threading;
using System.Reflection;
using System.IO;
using MS.Internal.PtsHost;
using Microsoft.Win32;
using UOMachine.Tree;
using UOMachine.Utility;
using UOMachine.Data;
using UOMachine.Resources;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.CodeCompletion;

namespace UOMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        private About aboutWindow;
        private OptionsWindow optionsWindow;
        private AssemblyPicker assemblyPicker;
        private static OptionsData myCurrentOptions;
        public static OptionsData CurrentOptions
        {
            get { return myCurrentOptions; }
        }
        private const double myButtonScaleFactor = .9, myDisabledOpacity = .5;
        private static DropShadowEffect myDropShadow;
        private static double myStartButtonWidth, myStopButtonWidth, myAddButtonWidth, myRazorButtonWidth, mySteamButtonWidth;
        private static Thickness myStartButtonMargin, myStopButtonMargin, myAddButtonMargin, myRazorButtonMargin, mySteamButtonMargin;
        private const string titleString = "UO Machine Alpha 4";
        private const string titleSpace = "                  ";
        private string fileTitleString = "";
        private string fileName = "";
        private delegate void dUpdateButtonStatus( Button button, bool IsEnabled );
        private delegate void dUpdateLabel( Label label, string content );
        private CSharpCompletion codeCompletion;

        private object myOptionWaitObject = new object();
        private bool myIsWaitingForOptions = false;
        private bool waitingForOptions
        {
            set { myIsWaitingForOptions = value; }
            get { return ThreadHelper.VolatileRead( ref myIsWaitingForOptions ); }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Initialize();
        }

        private void Initialize()
        {
            myDropShadow = new DropShadowEffect();
            myDropShadow.ShadowDepth = 0;
            myDropShadow.BlurRadius = 8;
            myDropShadow.Color = Colors.Black;
            myStartButtonWidth = startButton.Width;
            myStopButtonWidth = stopButton.Width;
            myAddButtonWidth = addButton.Width;
            myRazorButtonWidth = razorButton.Width;
            myStartButtonMargin = startButton.Margin;
            myStopButtonMargin = stopButton.Margin;
            myAddButtonMargin = addButton.Margin;
            myRazorButtonMargin = razorButton.Margin;
            mySteamButtonWidth = steamButton.Width;
            mySteamButtonMargin = steamButton.Margin;
            stopButton.IsEnabled = false;
            razorButton.IsEnabled = false;
            addButton.IsEnabled = false;
            stopButton.Opacity = myDisabledOpacity;
            Closing += new CancelEventHandler( MainWindow_Closing );
            TreeViewUpdater.Initialize( clientTreeView );
            OptionsWindow.OptionsChangedEvent += new OptionsWindow.dOptionsChanged( OptionsWindow_OptionsChangedEvent );
            OptionsWindow.OptionsCancelledEvent += new OptionsWindow.dOptionsCancelled( OptionsWindow_OptionsCancelledEvent );
            ScriptCompiler.ScriptFinishedEvent += new ScriptCompiler.dScriptFinished( ScriptCompiler_ScriptFinished );
            aboutWindow = new About();
            optionsWindow = new OptionsWindow();
            myCurrentOptions = OptionsData.Deserialize( "options.xml" );
            CheckOptions( myCurrentOptions );
            if (!UOM.Initialize( this ))
            {
                MessageBox.Show( Strings.Errorinitializing, Strings.Error );
                UOM.Dispose();
                UOM.ShutDown();
                return;
            }
            PrepareTextEditor();
            razorButton.IsEnabled = true;
            addButton.IsEnabled = true;
        }

        private void OptionsWindow_OptionsCancelledEvent()
        {
            if (waitingForOptions)
            {
                MessageBox.Show( Strings.Youmustentervalidoptions, Strings.Error );
                waitingForOptions = false;
            }
        }

        private void CheckOptions( OptionsData optionsData )
        {
            if (!optionsData.IsValid())
            {
                waitingForOptions = true;
                MessageBox.Show( Strings.Invalidvaluepresentinoptions, Strings.Error );
                optionsWindow.LoadOptions( OptionsData.CreateDefault() );
                optionsWindow.Show();
                while (waitingForOptions)
                {
                    try { Application.Current.Dispatcher.Invoke( DispatcherPriority.Background, new ThreadStart( delegate { } ) ); }
                    catch { }
                    Thread.Sleep( 50 );
                }
            }
        }

        private void MainWindow_Closing( object sender, CancelEventArgs e )
        {
            UOM.SetStatusLabel( Strings.Closing );
            UOM.Dispose();
            aboutWindow.CancelEnabled = false;
            optionsWindow.CancelEnabled = false;
            aboutWindow.Close();
            optionsWindow.Close();
            if (assemblyPicker != null)
            {
                assemblyPicker.CancelEnabled = false;
                assemblyPicker.Close();
            }
            App.Current.Shutdown();
        }

        private void PrepareTextEditor()
        {
            codeCompletion = new ICSharpCode.CodeCompletion.CSharpCompletion();
            codeCompletion.AddAssembly( "UOMachine.exe" );
            scriptTextBox.Completion = codeCompletion;
            scriptTextBox.FontFamily = new FontFamily( "Consolas" );
            scriptTextBox.FontSize = 12;
            scriptTextBox.TextArea.DefaultInputHandler.NestedInputHandlers.Add( new SearchInputHandler( scriptTextBox.TextArea ) );
            FileNew_Click( null, null );
            scriptTextBox.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition( "C#" );
            scriptTextBox.Options = myCurrentOptions.TextEditorOptions;
        }

        private void OptionsWindow_OptionsChangedEvent( OptionsData optionsData )
        {
            if (waitingForOptions)
            {
                if (optionsData.IsValid())
                {
                    waitingForOptions = false;
                }
                else
                {
                    MessageBox.Show( Strings.Invalidvaluepresentinoptions, Strings.Error );
                    optionsWindow.Show();
                }
            }
            if (optionsData.CacheLevel != myCurrentOptions.CacheLevel)
                Map.Initialize( optionsData.UOFolder, optionsData.CacheLevel );
            myCurrentOptions = optionsData;
            scriptTextBox.Options = optionsData.TextEditorOptions;
        }

        private void ScriptCompiler_ScriptFinished()
        {
            UpdateButtonStatus( startButton, true );
            UpdateButtonStatus( stopButton, false );
            UOM.SetStatusLabel( Strings.Scriptstopped );
        }

        private static void myUpdateLabel( Label label, string content ) { label.Content = content; }

        public static void UpdateLabel( Label label, string content )
        {
            if (label.CheckAccess()) myUpdateLabel( label, content );
            else label.Dispatcher.BeginInvoke( new dUpdateLabel( myUpdateLabel ), new object[] { label, content } );
        }

        private void myUpdateButtonStatus( Button button, bool IsEnabled )
        {
            button.IsEnabled = IsEnabled;
            if (IsEnabled) { button.Opacity = 1; }
            else { button.Opacity = myDisabledOpacity; }
        }

        private void UpdateButtonStatus( Button button, bool IsEnabled )
        {
            if (button.CheckAccess()) myUpdateButtonStatus( button, IsEnabled );
            else button.Dispatcher.BeginInvoke( new dUpdateButtonStatus( myUpdateButtonStatus ), new object[] { button, IsEnabled } );
        }

        private static void ButtonUp( Button button, double width, Thickness margin )
        {
            button.Width = width;
            button.Height = width;
            button.Margin = margin;
        }

        private static void ButtonDown( Button button )
        {
            //originally implemented in XAML but I prefer this
            double w = button.Width;
            double h = button.Height;
            double positionOffset = button.Width - (button.Width * myButtonScaleFactor);
            Thickness t = button.Margin;
            button.Width *= myButtonScaleFactor;
            button.Height *= myButtonScaleFactor;
            t.Left += positionOffset;
            t.Top += positionOffset;
            button.Margin = t;
            //Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void startButton_Click( object sender, RoutedEventArgs e )
        {
            if (ScriptCompiler.Compile( Assembly.GetExecutingAssembly().Location, scriptTextBox.Text ))
            {
                UOM.SetStatusLabel( Strings.Scriptrunning );
                startButton.IsEnabled = false;
                startButton.Opacity = myDisabledOpacity;
                stopButton.IsEnabled = true;
                stopButton.Opacity = 1;
            }
            else UOM.SetStatusLabel( Strings.Compilererror );
        }

        private void addButton_Click( object sender, RoutedEventArgs e )
        {
            int index;
            if (ClientLauncher.Launch( MainWindow.CurrentOptions, out index ))
            {
                UOM.SetStatusLabel( Strings.Clientstarted );
            }
            else
            {
                UOM.SetStatusLabel( Strings.Errorstartingclient );
                MessageBox.Show( Strings.Errorstartingclient, Strings.Error );
            }

        }

        private void process_Exited( object sender, EventArgs e )
        {
            UOM.SetStatusLabel( Strings.Clientexited );
        }

        private void stopButton_Click( object sender, RoutedEventArgs e )
        {
            UOM.SetStatusLabel( Strings.Stoppingscript );
            ScriptCompiler.StopScript();
            Events.General.ClearEvents();
            Events.IncomingPackets.ClearEvents();
            Events.OutgoingPackets.ClearEvents();
            Events.LowLevel.ClearEvents();
            startButton.IsEnabled = true;
            startButton.Opacity = 1;
            stopButton.IsEnabled = false;
            stopButton.Opacity = myDisabledOpacity;
            UOM.SetStatusLabel( Strings.Ready );
        }

        private void razorButton_Click( object sender, RoutedEventArgs e )
        {
            int index;
            razorButton.IsEnabled = false;
            addButton.IsEnabled = false;
            addButton.Opacity = myDisabledOpacity;
            razorButton.Opacity = myDisabledOpacity;
            RazorLauncher.Launch( myCurrentOptions, out index );
            razorButton.IsEnabled = true;
            addButton.IsEnabled = true;
            addButton.Opacity = 1;
            razorButton.Opacity = 1;
        }

        private void startButton_MouseEnter( object sender, MouseEventArgs e )
        {
            startButton.Effect = myDropShadow;
        }

        private void startButton_MouseLeave( object sender, MouseEventArgs e )
        {
            startButton.Effect = null;
        }

        private void stopButton_MouseEnter( object sender, MouseEventArgs e )
        {
            stopButton.Effect = myDropShadow;
        }

        private void stopButton_MouseLeave( object sender, MouseEventArgs e )
        {
            stopButton.Effect = null;
        }

        private void addButton_MouseEnter( object sender, MouseEventArgs e )
        {
            addButton.Effect = myDropShadow;
        }

        private void addButton_MouseLeave( object sender, MouseEventArgs e )
        {
            addButton.Effect = null;
        }

        private void razorButton_MouseEnter( object sender, MouseEventArgs e )
        {
            razorButton.Effect = myDropShadow;
        }

        private void razorButton_MouseLeave( object sender, MouseEventArgs e )
        {
            razorButton.Effect = null;
        }

        private void startButton_PreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            ButtonDown( startButton );
        }

        private void startButton_PreviewMouseUp( object sender, MouseButtonEventArgs e )
        {
            ButtonUp( startButton, myStartButtonWidth, myStartButtonMargin );
            if (e.ChangedButton != MouseButton.Left) startButton_Click( sender, e );
        }

        private void stopButton_PreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            ButtonDown( stopButton );
        }

        private void stopButton_PreviewMouseUp( object sender, MouseButtonEventArgs e )
        {
            ButtonUp( stopButton, myStopButtonWidth, myStopButtonMargin );
            if (e.ChangedButton != MouseButton.Left) stopButton_Click( sender, e );
        }

        private void addButton_PreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            ButtonDown( addButton );
        }

        private void addButton_PreviewMouseUp( object sender, MouseButtonEventArgs e )
        {
            ButtonUp( addButton, myAddButtonWidth, myAddButtonMargin );
            if (e.ChangedButton != MouseButton.Left) addButton_Click( sender, e );
        }

        private void razorButton_PreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            ButtonDown( razorButton );
        }

        private void razorButton_PreviewMouseUp( object sender, MouseButtonEventArgs e )
        {
            ButtonUp( razorButton, myRazorButtonWidth, myRazorButtonMargin );
            if (e.ChangedButton != MouseButton.Left) razorButton_Click( sender, e );
        }

        private void FileNew_Click( object sender, RoutedEventArgs e )
        {
            scriptTextBox.Text = Properties.Resources.DefaultScript;
            fileName = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\Untitled.cs";
            fileTitleString = "Untitled.cs";
            MenuItem parent = (MenuItem)menu1.Items[0];
            MenuItem child = (MenuItem)parent.Items[2];
            child.IsEnabled = false;
            UpdateWindowText();
            scriptTextBox.Document.FileName = fileName;
        }

        private void FileOpen_Click( object sender, RoutedEventArgs e )
        {
            OpenFileDialog OFD = new OpenFileDialog();
            OFD.CheckFileExists = true;
            OFD.CheckPathExists = true;
            OFD.Filter = "(*.txt, *.cs)|*.txt;*.cs|(*.*)|*.*";
            OFD.FileOk += new CancelEventHandler( OFD_FileOk );
            OFD.ShowDialog();
        }

        private void OFD_FileOk( object sender, CancelEventArgs e )
        {
            OpenFileDialog OFD = (OpenFileDialog)sender;
            if (OFD.FileName != "")
            {
                try
                {
                    //scriptTextBox.Text = File.ReadAllText(OFD.FileName);
                    scriptTextBox.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition( "C#" );
                    scriptTextBox.OpenFile( OFD.FileName );
                }
                catch (IOException)
                {
                    MessageBox.Show( Strings.Erroropeningfile );
                    return;
                }
                MenuItem parent = (MenuItem)menu1.Items[0];
                MenuItem child = (MenuItem)parent.Items[2];
                child.IsEnabled = true;
                fileName = OFD.FileName;
                fileTitleString = OFD.SafeFileName;
                UpdateWindowText();
            }
        }

        private void FileSave_Click( object sender, RoutedEventArgs e )
        {
            if (fileName != "")
            {
                try { File.WriteAllText( fileName, scriptTextBox.Text, Encoding.UTF8 ); }
                catch (IOException)
                {
                    MessageBox.Show( Strings.Errorsavingfile );
                }
            }
            else FileSaveAs_Click( null, null );
        }

        private void FileSaveAs_Click( object sender, RoutedEventArgs e )
        {
            SaveFileDialog SFD = new SaveFileDialog();
            SFD.CheckPathExists = true;
            if (fileName != "") SFD.FileName = fileName;
            SFD.DefaultExt = ".cs";
            SFD.Filter = "(*.cs)|*.cs|(*.*)|*.*";
            SFD.FileOk += new CancelEventHandler( SFD_FileOk );
            SFD.ShowDialog();
        }

        private void SFD_FileOk( object sender, CancelEventArgs e )
        {
            SaveFileDialog SFD = (SaveFileDialog)sender;
            if (SFD.FileName != "")
            {
                try { File.WriteAllText( SFD.FileName, scriptTextBox.Text, Encoding.UTF8 ); }
                catch (IOException)
                {
                    MessageBox.Show( Strings.Errorsavingfile );
                    return;
                }
                MenuItem parent = (MenuItem)menu1.Items[0];
                MenuItem child = (MenuItem)parent.Items[2];
                child.IsEnabled = true;
                fileName = SFD.FileName;
                fileTitleString = SFD.SafeFileName;
                UpdateWindowText();
            }
        }

        private void FileExit_Click( object sender, RoutedEventArgs e )
        {
            Application.Current.Shutdown();
        }

        private void UpdateWindowText()
        {
            Title = titleString + titleSpace + "( " + fileTitleString + " )";
        }

        private void HelpAbout_Click( object sender, RoutedEventArgs e )
        {
            aboutWindow.Show();
        }

        private void EditOptions_Click( object sender, RoutedEventArgs e )
        {
            optionsWindow.LoadOptions( myCurrentOptions );
            optionsWindow.Show();
        }

        private void FormatDocument_Click( object sender, RoutedEventArgs e )
        {
            scriptTextBox.Text = DocumentHelper.Format( scriptTextBox.Text, scriptTextBox.Options.IndentationString );
            TextEditorOptions teo = scriptTextBox.Options;
        }

        private void AddReference_Click( object sender, RoutedEventArgs e )
        {
            if (assemblyPicker == null)
            {
                assemblyPicker = new AssemblyPicker();
                assemblyPicker.AssemblySelectedEvent += new AssemblyPicker.dAssemblySelected( assemblyPicker_AssemblySelectedEvent );
            }
            assemblyPicker.Show();
        }

        private void assemblyPicker_AssemblySelectedEvent( string fileName )
        {
            scriptTextBox.Text = "/* <AREF = \"" + fileName + "\"> */\r\n" + scriptTextBox.Text;
        }

        private void steamButton_MouseEnter( object sender, MouseEventArgs e )
        {
            steamButton.Effect = myDropShadow;
        }

        private void steamButton_MouseLeave( object sender, MouseEventArgs e )
        {
            steamButton.Effect = null;
        }

        private void steamButton_PreviewMouseDown( object sender, MouseButtonEventArgs e )
        {
            ButtonDown( steamButton );
        }

        private void steamButton_PreviewMouseUp( object sender, MouseButtonEventArgs e )
        {
            ButtonUp( steamButton, mySteamButtonWidth, mySteamButtonMargin );
            if (e.ChangedButton != MouseButton.Left) steamButton_Click( sender, e );

        }

        private void steamButton_Click( object sender, RoutedEventArgs e )
        {
            int index;
            razorButton.IsEnabled = false;
            addButton.IsEnabled = false;
            addButton.Opacity = myDisabledOpacity;
            razorButton.Opacity = myDisabledOpacity;
            UOMachine.Misc.SteamLauncher.Launch( myCurrentOptions, out index );
            razorButton.IsEnabled = true;
            addButton.IsEnabled = true;
            addButton.Opacity = 1;
            razorButton.Opacity = 1;
        }

        private void checkUpdate_Click( object sender, RoutedEventArgs e )
        {

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WorkingDirectory = System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location );
            startInfo.FileName = System.IO.Path.Combine( System.IO.Path.GetDirectoryName( System.Reflection.Assembly.GetExecutingAssembly().Location ), "Updater.exe" );
            Win32.SafeProcessHandle hProcess;
            Win32.SafeThreadHandle hThread;
            uint pid, tid;
            UOM.SetStatusLabel( "Status : Launching Updater" );
            Win32.CreateProcess( startInfo, false, out hProcess, out hThread, out pid, out tid );
        }
    }
}
