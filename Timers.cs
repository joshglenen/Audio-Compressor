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
        private int curSetVol = defaultVol;
        #region Timer Preferences Set or Altered

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
            }

            catch
            {
                Debug.WriteLine("Casting issue in intimed event");
                
            }
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
            VolumeMixer.SetVol(Convert.ToInt32(defaultVol * attenuation));
            if (hold) myTimer.Interval = holdTime;
            myTimer.Enabled = true;
        }

        #endregion

        #region Timer Activated Operations

        /*
        private double dBtoPeakVolume(double db)
        {
            double val;
            return val = Math.Pow(10, (db / 10));
        }

        private double peakVolumeToDB(double peakVolume)
        {
            double val;
            return val = 10 * Math.Log10(peakVolume / 1);
        }
        */

        //checks or changes the volume
        private void checkVolume(float val)
        {
            //reset timer interval
            myTimer.Interval = timerInterval;

            //in case the user clicks stop while calculation is taking place
            if (!myTimer.Enabled) return;

            //true gain mode
            if (trueGain)
            {
                try
                {
                    //apply upper attenuation
                    if (val > UpThresh)
                    {

                        curSetVol = Convert.ToInt32(defaultVol * (1 - ((val - UpThresh) * (1 - attenuation))));
                        //Debug.WriteLine("Signal attenuated to " + i.ToString());
                        VolumeMixer.SetVol(curSetVol);

                    }

                    //apply lower attenuation
                    else if (val < DownThresh)
                    {
                        curSetVol = Convert.ToInt32(defaultVol * (1 + ((DownThresh - val) * (1 - attenuation))));
                        //Debug.WriteLine("Signal amplified to " + i.ToString());
                       VolumeMixer.SetVol(curSetVol);
                    }


                    //keep original signal as is
                    else
                    {
                        if (curSetVol == defaultVol) return;
                        curSetVol = defaultVol;
                        VolumeMixer.SetVol(Convert.ToInt32(curSetVol));

                    }
                }

                catch
                {
                    Debug.WriteLine("TRUE GAIN: Threading issue");


                }

            }

            //original mode
            else if (((!volLowered) && (val >= UpThresh)) || ((volLowered) && (val <= DownThresh)))
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
        
        #endregion
    }
}
