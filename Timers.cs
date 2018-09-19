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
            VolumeMixer.SetVol(Convert.ToInt32(defaultVol * applyGain(percentMod)));
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
    }
}
