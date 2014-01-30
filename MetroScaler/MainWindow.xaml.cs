using MetroScaler.EdidOverride;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private List<EdidMonitor> allMonitors;
        private EdidMonitor selectedMonitor;

        private const double MIN_INCHES = 8;
        private const double MAX_INCHES = 36;

        public MainWindow()
        {
            InitializeComponent();

            allMonitors = EdidOverrideUtils.GetMonitorList();

            foreach(EdidMonitor monitor in allMonitors)
            {
                this.monitors_combobox.ItemsSource = allMonitors;
                this.monitors_combobox.SelectedIndex = 0;
            }
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
                this.selectedMonitor.ResetEdidOverride();
                this.slider.Value = inchesToSlider(this.selectedMonitor.Inches);
                this.showRestartDialog("The screen size has been reset. Restart your computer to see the changes.");
            }
            catch (Exception c)
            {
                Debug.WriteLine(c.Message);
                MessageBoxResult result = MessageBox.Show("Could not write to the registry. Make sure to run as administrator.", appName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnScale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.selectedMonitor.ScaleToInches(this.sliderToInches(this.slider.Value));
                this.selectedMonitor.WriteEdidOverride();

                this.showRestartDialog("The screen size for " + this.selectedMonitor.Name + " has been set to " + this.sliderToInches(this.slider.Value).ToString() + " inches. Restart your computer to see the changes.");
            }
            catch (Exception c)
            {
                Debug.WriteLine(c.Message);
                MessageBoxResult result = MessageBox.Show("Could not write to the registry. Make sure to run as administrator.", appName, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void showRestartDialog(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, appName, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private double sliderToInches(double sliderVal)
        {
            double round = 0.1;
            return ((int)((MIN_INCHES + sliderVal * (MAX_INCHES - MIN_INCHES)) / round)) * round;
        }
        private double inchesToSlider(double inches)
        {
            return (inches - MIN_INCHES) / (MAX_INCHES - MIN_INCHES);
        }

        private void monitors_combobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            this.selectedMonitor = (EdidMonitor)cb.SelectedItem;
            this.slider.Value = inchesToSlider(this.selectedMonitor.Inches);
        }
    }
}
