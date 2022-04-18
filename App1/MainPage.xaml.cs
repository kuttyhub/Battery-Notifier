using System;
using Windows.Devices.Power;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;

namespace App1
{
    public sealed partial class MainPage : Page
    {
        public int CurrentBatteryPecentage;
        public int CurrentSelctedMinBatteryPercentage = 20;
        public int CurrentSelctedMaxBatteryPercentage = 98;
        public MainPage()
        {
            this.InitializeComponent();
            Battery.AggregateBattery.ReportUpdated += AggregateBattery_ReportUpdated;

            RequestAggregateBatteryReport();


            MinBatterySlider.Value = CurrentSelctedMinBatteryPercentage;
            MinBatterySlider.Maximum = 49;

            MaxBatterySlider.Value = CurrentSelctedMaxBatteryPercentage;
            MaxBatterySlider.Minimum = 50;

        }
        private void MinBatterySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                CurrentSelctedMinBatteryPercentage = (int)slider.Value;
                MinBatteryPercentageText.Text = "Min--> " + CurrentSelctedMinBatteryPercentage.ToString() + "%";

            }
        }
        private void MaxBatterySlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (sender is Slider slider)
            {
                CurrentSelctedMaxBatteryPercentage = (int)slider.Value;
                MaxBatteryPercentageText.Text = "Max--> " + CurrentSelctedMaxBatteryPercentage.ToString() + "%";

            }
        }

        private void ShowToastNotification(String BatteryPercentage, String slogan)
        {
            // Requires Microsoft.Toolkit.Uwp.Notifications NuGet package version 7.0 or greater
            System.Diagnostics.Debug.WriteLine(slogan);
            new ToastContentBuilder()
                .AddArgument("conversationId", 9813)
                .AddText("Battery reach " + BatteryPercentage + "% !")
                .AddText(slogan)
                .Show(); // Not seeing the Show() method? Make sure you have version 7.0, and if you're using .NET 5, your TFM must be net5.0-windows10.0.17763.0 or greater
        }

        private void BtnToast(object sender, RoutedEventArgs e)
        {
            ShowToastNotification(CurrentBatteryPecentage.ToString(), "disconnect charger !");
        }

        private void GetBatteryReport(object sender, RoutedEventArgs e)
        {
            // Clear UI
            BatteryReportPanel.Children.Clear();

            // Request aggregate battery report
            RequestAggregateBatteryReport();
        }


        private void RequestAggregateBatteryReport()
        {
            // Create aggregate battery object
            var aggBattery = Battery.AggregateBattery;

            // Get report
            var report = aggBattery.GetReport();
            var result = (double)((Convert.ToDouble(report.RemainingCapacityInMilliwattHours) / Convert.ToDouble(report.FullChargeCapacityInMilliwattHours)) * 100);

            // Update BatteryPercentage
            CurrentBatteryPecentage = (int)result;
            System.Diagnostics.Debug.WriteLine("Requesting Battery ...!"+result.ToString());


            // checking battery status and show toast message

            if (report.Status.ToString().TrimEnd() == "Discharging" && CurrentBatteryPecentage == CurrentSelctedMinBatteryPercentage)
            {
                ShowToastNotification(CurrentBatteryPecentage.ToString(), "Please Connect  the Charger");
            }
            else if (CurrentBatteryPecentage == CurrentSelctedMaxBatteryPercentage)
            {
                ShowToastNotification(CurrentBatteryPecentage.ToString(), "Please Disconnect Charger !");
            }

            // Update UI
            AddReportUI(BatteryReportPanel, report, aggBattery.DeviceId);
        }
        private static void AddReportUI(StackPanel sp, BatteryReport report, string DeviceID)
        {
            // Create battery report UI
            TextBlock txt1 = new TextBlock { Text = "Device ID: " + DeviceID };
            txt1.FontSize = 15;
            txt1.Margin = new Thickness(0, 15, 0, 0);
            txt1.TextWrapping = TextWrapping.WrapWholeWords;

            TextBlock txt2 = new TextBlock { Text = "Battery status: " + report.Status.ToString() };
            txt2.FontStyle = Windows.UI.Text.FontStyle.Italic;
            txt2.Margin = new Thickness(0, 0, 0, 15);

            TextBlock txt3 = new TextBlock { Text = "Charge rate (mW): " + report.ChargeRateInMilliwatts.ToString() };
            TextBlock txt4 = new TextBlock { Text = "Design energy capacity (mWh): " + report.DesignCapacityInMilliwattHours.ToString() };
            TextBlock txt5 = new TextBlock { Text = "Fully-charged energy capacity (mWh): " + report.FullChargeCapacityInMilliwattHours.ToString() };
            TextBlock txt6 = new TextBlock { Text = "Remaining energy capacity (mWh): " + report.RemainingCapacityInMilliwattHours.ToString() };

            // Create energy capacity progress bar & labels
            TextBlock pbLabel = new TextBlock { Text = "Percent remaining energy capacity" };
            pbLabel.Margin = new Thickness(0, 10, 0, 5);
            pbLabel.FontFamily = new FontFamily("Segoe UI");
            pbLabel.FontSize = 11;

            ProgressBar pb = new ProgressBar
            {
                Margin = new Thickness(0, 5, 0, 0),
                Width = 200,
                Height = 10,
                IsIndeterminate = false,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            TextBlock pbPercent = new TextBlock { };
            pbPercent.Margin = new Thickness(0, 5, 0, 10);
            pbPercent.FontFamily = new FontFamily("Segoe UI");
            pbLabel.FontSize = 11;

            // Disable progress bar if values are null
            if ((report.FullChargeCapacityInMilliwattHours == null) ||
                (report.RemainingCapacityInMilliwattHours == null))
            {
                pb.IsEnabled = false;
                pbPercent.Text = "N/A";
            }
            else
            {
                pb.IsEnabled = true;
                pb.Maximum = Convert.ToDouble(report.FullChargeCapacityInMilliwattHours);
                pb.Value = Convert.ToDouble(report.RemainingCapacityInMilliwattHours);
                pbPercent.Text = ((pb.Value / pb.Maximum) * 100).ToString("F2") + "%";
            }

            // Add controls to stackpanel
            sp.Children.Add(txt1);
            sp.Children.Add(txt2);
            sp.Children.Add(txt3);
            sp.Children.Add(txt4);
            sp.Children.Add(txt5);
            sp.Children.Add(txt6);
            sp.Children.Add(pbLabel);
            sp.Children.Add(pb);
            sp.Children.Add(pbPercent);
        }

        async private void AggregateBattery_ReportUpdated(Battery sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //System.Diagnostics.Debug.WriteLine("fetching battery status");
                // Clear UI
                BatteryReportPanel.Children.Clear();

                RequestAggregateBatteryReport();
            });

        }
    }
}
