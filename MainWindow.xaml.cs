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
        public bool trueGain = false;
        public double UpThresh = 0.80;
        public int samples = 10;
        public int timerInterval = 50;
        public double DownThresh = 0.40;
        public double holdTime = 500;
        public double attenuation = 0.50;
        public double attackVal = 10;
        public double releaseVal = 10;
        public static int defaultVol = 2;

        #region Setup
        private void SetProgressBar()
        {
            Emu.Minimum = 0;
            Emu.Maximum = 1;
            Emu_Copy.Minimum = 0;
            Emu_Copy.Maximum = 1;
        }
        private void SetVolumeMixer()
        {
            samples = Convert.ToInt32(advancedControls.samps.Text);
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
        #endregion
        
        #region GUI Interacted

        private void vol_changed_slider(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            DV.Text = Math.Floor(preamp.Value).ToString();
            defaultVol = Convert.ToInt32(Convert.ToDouble(DV.Text));
            VolumeMixer.SetVol(defaultVol);
        }

        private void attenuation_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            PR.Text = (ratio.Value).ToString();
            attenuation = Convert.ToDouble(PR.Text);
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

            DownThresh = Convert.ToDouble(advancedControls.LO.Text)/100;
            UpThresh = Convert.ToDouble(advancedControls.UP.Text)/100;
            attackVal = Math.Floor(Convert.ToDouble(advancedControls.AT.Text));
            releaseVal = Math.Floor(Convert.ToDouble(advancedControls.RE.Text));
            timerInterval = Convert.ToInt32(Math.Floor(Convert.ToDouble(advancedControls.IN.Text)));
            myTimer.Enabled = true;
            RunProg.Content = "Stop";
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
            try
            {
                advancedControls.Show();
            }
            catch
            {
                advancedControls = new AdvancedControls();
                advancedControls.Show();
            }
            attackTimer.Enabled = false;
            releaseTimer.Enabled = false;
            myTimer.Enabled = false;
            RunProg.Content = "Start";
        }

        #endregion

        //recieve changes from advanced controls when second window is closed
        private void advancedControls_SubmitClicked(object sender, EventArgs e)
        {
            DownThresh = advancedControls.aDownThresh/100;
            UpThresh = advancedControls.aUpThresh/100;
            attackVal = advancedControls.aattackVal;
            samples = advancedControls.asamples;
            trueGain = advancedControls.trueGain;
            releaseVal = advancedControls.areleaseVal;
            timerInterval = advancedControls.atimerInterval;
            myVolumeMixer = new VolumeMixer(samples);
            VolumeMixer.SetVol(defaultVol);

        }

        //closes app when window is closed
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            System.Windows.Application.Current.Shutdown();
        }

        public MainWindow()
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
            {
                Close();
                return;

            } //Allows only one persistance of the program to run


            InitializeComponent();
            SetWindow2();
            SetProgressBar();
            SetVolumeMixer();
            SetTimer();

        }

    }
}
