using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows;
using System.Timers;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace Audio_Dynamic_Range_Compressor
{
    public partial class MainWindow : Window
    {
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
        //waits a set time before immediately decreasing the volume
        private void OnTimedEventRelease(object sender, ElapsedEventArgs e)
        {
            volLowered = !volLowered;
            VolumeMixer.SetVol(Convert.ToInt32(defaultVol));
            if (hold) myTimer.Interval = holdTime;
            myTimer.Enabled = true;
        }
        //waits a set time before immediately increasing the volume
        private void OnTimedEventAttack(object sender, ElapsedEventArgs e)
        {
            volLowered = !volLowered;
            VolumeMixer.SetVol(Convert.ToInt32(defaultVol * dBtoPeakVolume(upperGain)));
            if (hold) myTimer.Interval = holdTime;
            myTimer.Enabled = true;
        }
        //occurs many times per second -> performance issues may arise at low delta t
        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            try
            {
                //get new data from primary audio stream
                float av = 0; float buf = 0;
                myVolumeMixer.ReadToBuffer();
                Dispatcher.Invoke(new Action(() => { buf = myVolumeMixer.bufferMemory.ReadLast(); }), DispatcherPriority.ContextIdle);
                Dispatcher.Invoke(new Action(() => { Emu.Value = buf; }), DispatcherPriority.ContextIdle);

                //check the volume to see if it passes threshold conditions
                if (myVolumeMixer.bufferMemory.dataLoaded)
                {
                    Dispatcher.Invoke(new Action(() => { av = myVolumeMixer.bufferMemory.ReadAverage(); }), DispatcherPriority.ContextIdle);
                    Dispatcher.Invoke(new Action(() => { Emu_Copy.Value = av; }), DispatcherPriority.ContextIdle);
                    if (averagePreference) Dispatcher.Invoke(new Action(() => { checkVolume(av); }), DispatcherPriority.ContextIdle);
                    else Dispatcher.Invoke(new Action(() => { checkVolume(buf); }), DispatcherPriority.ContextIdle);
                }
                else Dispatcher.Invoke(new Action(() => { checkVolume(buf); }), DispatcherPriority.ContextIdle);

                //reset the timer since the timer value is changed during the hold process
                myTimer.Interval = timerInterval;
            }

            catch(Exception ee)
            {
                Debug.WriteLine("Threading issue: " + ee.ToString());
            }
        }

        //checks or changes the volume
        private void checkVolume(float val)
        {
            if(unityMode)
            {
                setVolumeToUnity();
                return;
            }
            if (((!volLowered) && (val >= UpThresh)) || ((volLowered) && (val <= DownThresh)))
            {
                myTimer.Enabled = false;
                if (volLowered)
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

        private void setVolumeToUnity()
        {
            int wantToBe = defaultVol;
            double N = 0.25;
            int setTo = 0;
            double currentlyAt = 0;
            if((myVolumeMixer.bufferMemory.dataLoaded) && (averagePreference)) Dispatcher.Invoke(new Action(() => { currentlyAt = myVolumeMixer.bufferMemory.ReadAverage(); }), DispatcherPriority.ContextIdle);
            else Dispatcher.Invoke(new Action(() => { currentlyAt = myVolumeMixer.bufferMemory.ReadLast(); }), DispatcherPriority.ContextIdle);
            if (currentlyAt > N) setTo = Convert.ToInt32(defaultVol/currentlyAt);
            else setTo = Convert.ToInt32(defaultVol / N);
            if (setTo > 100) setTo = 100;
            if (setTo < 0) setTo = 0;
            //1 -> nothing
            //0 -> nothing
            //0.5 -> 1
            //0.25 -> 1
            //x = peak
            //y = new
            //k = current
            //maybe y= k/x

            //posible issue for low noises leading into loud noises with a very high volume, therefore need to 
            //raise the value of the minimum set point from 0 to N. N is only modifiable before compiling.

            VolumeMixer.SetVol(setTo);
        }
    }
}
