using MetroScaler.EdidOverride;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MetroScaler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string appName = "Metro Scaler";

        //HKEY_LOCAL_MACHINE->SOFTWARE->Microsoft->Windows->CurrentVersion->Explorer->Scaling
        private RegistryKey scalingRegisterKey;

        private string valueKey = "MonitorSize";


        public MainWindow()
        {
            InitializeComponent();
            this.slider.Value = 5.0;

            EdidOverrideUtils.test();
            /*
            try
            {
                this.scalingRegisterKey = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Scaling");
            }
            catch (Exception e)
            {
                MessageBoxResult result = MessageBox.Show("You have to start this application as an administrator", appName, MessageBoxButton.OK, MessageBoxImage.Warning);
                // Process message box results 
                switch (result)
                {
                    case MessageBoxResult.OK:
                        Application.Current.Shutdown();
                        break;
                }
            }
            */
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double inches = this.sliderToInches(e.NewValue);
            this.valueLabel.Content = inches + "\"";
            if (inches < 12.5)
            {
                this.preview.Source = Imaging.CreateBitmapSourceFromBitmap(MetroScaler.Properties.Resources._10);
            }
            else if (inches < 17.5)
            {
                this.preview.Source = Imaging.CreateBitmapSourceFromBitmap(MetroScaler.Properties.Resources._15);
            }
            else
            {
                this.preview.Source = Imaging.CreateBitmapSourceFromBitmap(MetroScaler.Properties.Resources._20);
            }
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                scalingRegisterKey.DeleteValue(this.valueKey);
            }
            catch (Exception c)
            {
                MessageBoxResult result = MessageBox.Show("Could not set the value in the registry. Try again.", appName, MessageBoxButton.OK, MessageBoxImage.Warning);

            }
            this.showRestartDialog("The scaling setting has been reset, restart your computer to see the changes.");
        }

        private void btnScale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                scalingRegisterKey.SetValue(this.valueKey, this.sliderToInches(this.slider.Value).ToString(), RegistryValueKind.String);
            }
            catch (Exception c)
            {
                MessageBoxResult result = MessageBox.Show("Could not set value in the registry. Try again.", appName, MessageBoxButton.OK, MessageBoxImage.Warning);
                
            }

            this.showRestartDialog("The scaling has been set to " + this.sliderToInches(this.slider.Value).ToString() + " inches, restart your computer to see the changes.");
        }

        private void showRestartDialog(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, appName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private double sliderToInches(double sliderVal)
        {
            double min = 5;
            double max = 20;
            double round = 0.5;

            return ((int)((min + (sliderVal / 10) * max) / round)) * round;
        }
    }
}
