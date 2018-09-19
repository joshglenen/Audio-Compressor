using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Audio_Dynamic_Range_Compressor
{
    /// <summary>
    /// Interaction logic for AdvancedControls.xaml
    /// </summary>
    public partial class AdvancedControls : Window
    {
        public event EventHandler SubmitClicked;
        public double aUpThresh;
        public int atimerInterval;
        public int asamples;
        public double aDownThresh;
        public double aholdTime;
        public double aattackVal;
        public double areleaseVal;

        public AdvancedControls()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            if (SubmitClicked != null)
            {
                aDownThresh = Convert.ToDouble(LO.Text);
                aUpThresh = Convert.ToDouble(UP.Text);
                asamples = Convert.ToInt32(Math.Floor(Convert.ToDouble(samps.Text)));
                aattackVal = Math.Floor(Convert.ToDouble(AT.Text));
                areleaseVal = Math.Floor(Convert.ToDouble(RE.Text));
                atimerInterval = Convert.ToInt32(Math.Floor(Convert.ToDouble(IN.Text)));
                SubmitClicked(this, new EventArgs());
            }
        }
        

        private void ut_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UP.Text = upperthreshold.Value.ToString();
        }

        private void lt_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LO.Text = lowthresh.Value.ToString();
        }

        private void at_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            AT.Text = attack.Value.ToString();
        }

        private void re_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RE.Text = release.Value.ToString();
        }

        private void in_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IN.Text = interval.Value.ToString();
        }

        private void ho_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Hold.Text = hold.Value.ToString();
        }

        private void sa_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            samps.Text = sampsslid.Value.ToString();
        }
    }
    
}
