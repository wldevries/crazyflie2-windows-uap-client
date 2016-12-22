using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CrazyflieClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private FlightController flightController;
        private CrazyflieController crazyflieController;

        private bool isCrazyfliePaired;
        private DispatcherTimer timer;

        public MainPage()
        {
            this.InitializeComponent();

            flightController = new FlightController(leftStick, rightStick);
            crazyflieController = new CrazyflieController(flightController);    
        }

        private async void onClick(object sender, RoutedEventArgs e)
        {
            if(connectionButton.Content.ToString() == "Connect")
            {
                isCrazyfliePaired = await crazyflieController.IsCrazyfliePaired();
                if(!isCrazyfliePaired)
                {
                    infoText.Text = "Error: Crazyflie not found. Please pair in settings->devices->bluetooth";
                }
                else
                {
                    infoText.Text = "";
                    connectionButton.Content = "Disconnect";
                    await crazyflieController.Start();

                    this.timer = new DispatcherTimer();
                    this.timer.Interval = TimeSpan.FromSeconds(.1);
                    this.timer.Tick += this.UpdatePacketsSent;
                    this.timer.Start();
                }
            }
            else if(connectionButton.Content.ToString() == "Disconnect")
            {
                infoText.Text = "";
                connectionButton.Content = "Connect";
                await crazyflieController.Stop();

                this.timer?.Stop();
                this.timer = null;
            }
        }

        private void UpdatePacketsSent(object sender, object e)
        {
            this.packetCount.Text = $"{this.crazyflieController.PacketsSent} packets sent";
        }

        private async void openBluetoothSettings(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings-bluetooth:", UriKind.RelativeOrAbsolute));
        }
    }
}
