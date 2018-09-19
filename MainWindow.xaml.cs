using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Timers;
using System.Windows.Threading;

//credits
//<div>Icons made by <a href="https://www.flaticon.com/authors/smashicons" title="Smashicons">Smashicons</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

namespace Audio_Dynamic_Range_Compressor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //static and private variables
        private bool hold = true;
        private AdvancedControls advancedControls; 
        private bool volLowered = false;
        private bool averagePreference = false;
        private System.Timers.Timer myTimer;
        private System.Timers.Timer attackTimer;
        private System.Timers.Timer releaseTimer;

        private VolumeMixer myVolumeMixer;

        //dynamic variables
        public double UpThresh = 0.80;
        public int samples = 10;
        public int timerInterval = 50;
        public double DownThresh = 0.40;
        public double holdTime = 500;
        public double percentMod = 0.50;
        public double attackVal = 10;
        public double releaseVal = 10;
        public static int defaultVol = 2;
        
        private void SetProgressBar()
        {
            Emu.Minimum = 0;
            Emu.Maximum = 1;
            Emu_Copy.Minimum = 0;
            Emu_Copy.Maximum = 1;
        }
        private void SetVolumeMixer()
        {
            myVolumeMixer = new VolumeMixer(samples);
            VolumeMixer.SetVol(defaultVol);
        }
        private void SetWindow2()
        {
            advancedControls = new AdvancedControls();
            //allows data transfer from options menu
            advancedControls.SubmitClicked += new
            EventHandler(advancedControls_SubmitClicked);
        }

        //closes app when window is closed
        protected override void OnClosed(EventArgs e)
        {

            base.OnClosed(e);

            System.Windows.Application.Current.Shutdown();
        }

        private void vol_changed_slider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DV.Text = Math.Floor(preamp.Value).ToString();
            defaultVol = Convert.ToInt32(Convert.ToDouble(DV.Text));
            VolumeMixer.SetVol(defaultVol);
        }

        private void g_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PR.Text = gain.Value.ToString();
            percentMod = Convert.ToDouble(PR.Text);
        }
        private double applyGain(double db)
        {
            double val;
            return val = Math.Pow(10, (db / 10));
        }
        public void RunProg_Click(object sender, RoutedEventArgs e)
        {
            if (advancedControls.IsVisible) return;
            if(myTimer.Enabled == true)
            {
                myTimer.Enabled = false;
                RunProg.Content = "Start";

                return;
            }
            RunProg.Content = "Stop";

            myTimer.Enabled = true;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            averagePreference = !averagePreference;
        }

        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            averagePreference = !averagePreference;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            VolumeMixer.SetVol(defaultVol);
            try
            {
                advancedControls.Show();
            }
            catch
            {
                advancedControls = new AdvancedControls();
                advancedControls.Show();
            }
            myTimer.Enabled = false;

        }

        //recieve changes from advanced controls when second window is closed
        private void advancedControls_SubmitClicked(object sender, EventArgs e)
        {
            DownThresh = advancedControls.aDownThresh/100;
            UpThresh = advancedControls.aUpThresh/100;
            attackVal = advancedControls.aattackVal;
            samples = advancedControls.asamples;
            releaseVal = advancedControls.areleaseVal;
            timerInterval = advancedControls.atimerInterval;
            myVolumeMixer = new VolumeMixer(samples);
            myTimer.Enabled = true;
            RunProg.Content = "Stop";
        }

        public MainWindow()
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Close();
                return;

            } //Allows only one persistance of the program to run


            InitializeComponent();
            SetProgressBar();
            SetVolumeMixer();
            SetTimer();
            SetWindow2();

        }

    }
}
