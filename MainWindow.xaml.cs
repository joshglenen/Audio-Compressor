using System;
using System.Diagnostics;
using System.Windows;
using System.Timers;
using System.Windows.Threading;

//credits
//<div>Icons made by <a href="https://www.flaticon.com/authors/smashicons" title="Smashicons">Smashicons</a> from <a href="https://www.flaticon.com/" title="Flaticon">www.flaticon.com</a> is licensed by <a href="http://creativecommons.org/licenses/by/3.0/" title="Creative Commons BY 3.0" target="_blank">CC 3.0 BY</a></div>

namespace DRWatchdogV2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //static and private variables
        private bool changeVol = false;
        private bool volLowered = false;
        private System.Timers.Timer myTimer;
        private System.Timers.Timer attackTimer;
        private System.Timers.Timer releaseTimer;
        private VolumeMixer myVolumeMixer;

        //dynamic variables
        public double UpThresh = 0.80;
        public int timerInterval = 50;
        public double DownThresh = 0.40;
        public double percentMod = 0.50;
        public double attackVal = 10;
        public double releaseVal = 10;
        public static int defaultVol = 2;

        //configure fundamental classes
        private void SetProgressBar()
        {
            Emu.Minimum = 0;
            Emu.Maximum = 1;
            Emu_Copy.Minimum = 0;
            Emu_Copy.Maximum = 1;
        }
        private void SetVolumeMixer()
        {
            myVolumeMixer = new VolumeMixer(10);
            VolumeMixer.SetVol(defaultVol);
        }
        private void SetTimer()
        {
            myTimer = new System.Timers.Timer(timerInterval);
            attackTimer = new System.Timers.Timer(attackVal);
            releaseTimer = new System.Timers.Timer(releaseVal);
            myTimer.Elapsed += OnTimedEvent;
            attackTimer.Elapsed += OnTimedEventAttack;
            releaseTimer.Elapsed += OnTimedEventRelease;
            myTimer.AutoReset = true;
            attackTimer.AutoReset = false;
            releaseTimer.AutoReset = false;
            myTimer.Enabled = false;
            attackTimer.Enabled = false;
            releaseTimer.Enabled = false;
        }
        private void OnTimedEventRelease(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine("Released");
            VolumeMixer.SetVol(Convert.ToInt32(defaultVol));
            myTimer.Enabled = true;
        }
        private void OnTimedEventAttack(object sender, ElapsedEventArgs e)
        {
            Debug.WriteLine("Attacked");
            VolumeMixer.SetVol(Convert.ToInt32(defaultVol * percentMod));
            myTimer.Enabled = true;
        }
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            myVolumeMixer.ReadToBuffer();
            Dispatcher.Invoke(new Action(() => { Emu.Value = myVolumeMixer.bufferMemory.ReadLast(); }), DispatcherPriority.ContextIdle);
            if(myVolumeMixer.bufferMemory.dataLoaded) Dispatcher.Invoke(new Action(() => { Emu_Copy.Value = myVolumeMixer.bufferMemory.ReadAverage(); }), DispatcherPriority.ContextIdle);
            Dispatcher.Invoke(new Action(() => { checkVolume(myVolumeMixer.bufferMemory.ReadLast()); }), DispatcherPriority.ContextIdle);
            myTimer.Interval = timerInterval;
        }

        //checks and changes volume
        private void checkVolume(float val)
        {
            if ((!volLowered) && (val >= UpThresh))
            {
                volLowered = !volLowered; changeVol = !changeVol;
            }
            else if ((volLowered) && (val <= DownThresh))
            {
                volLowered = !volLowered; changeVol = !changeVol;
            }
            if (changeVol)
            {
                myTimer.Enabled = false;
                changeVol = !changeVol;
                //note: volLowered has already been toggled
                if (!volLowered)
                {
                    //raise volume, need to check after delay 
                    releaseTimer.Interval = releaseVal;
                    releaseTimer.Enabled = true;
                }
                else
                {
                    //lower volume, need to check after delay 
                    attackTimer.Interval = attackVal;
                    attackTimer.Enabled = true;
                }
            }
        }
        

        //GUI
        public void RunProg_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            //TODO
            myTimer.Enabled = true;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            //TODO
        }
        private void Reset()
        {
            DownThresh = Convert.ToDouble(LO.Text);
            UpThresh = Convert.ToDouble(UP.Text);
            defaultVol = Convert.ToInt32(DV.Text);
            percentMod = Convert.ToDouble(PR.Text);
            attackVal = Convert.ToDouble(AT.Text);
            releaseVal = Convert.ToDouble(RE.Text);
            timerInterval = Convert.ToInt32(IN.Text);
            VolumeMixer.SetVol(defaultVol);
        }

        //Main
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
    }
    }
}
