using System.Threading;
using System.Threading.Tasks;

namespace CrazyflieClient
{
    /// <summary>
    /// This class implements the control of
    /// a crazyflie 2.0 quadcopter via BLE
    ///
    /// Uses an object which implements the IFlightController
    /// interface for determining flight commander setpoints
    /// </summary>
    public class CrazyflieController
    {
        private readonly IFlightController flightController;
        private CancellationTokenSource cancellationSource;
        private CrtpGattService gattService;

        private Task backgroundTask;

        private const int MaxThrust = 65536;

        /// <summary>
        /// Maximum pitch/roll value to send to the commander, in degrees
        /// </summary>
        public double MaxPitchRollRate { get; set; } = 30;

        /// <summary>
        /// Maximum yaw value to send to the commander, in degrees
        /// </summary>
        public double MaxYawRate { get; set; } = 200; // degrees per second

        /// <summary>
        /// Maximum roll value to send to the commander, in percent (0 to 1)
        /// </summary>
        public double MaxThrustPercent { get; set; } = 0.8;

        public CrazyflieController(IFlightController flightController)
        {
            this.flightController = flightController;
            gattService = new CrtpGattService();
        }

        public int PacketsSent { get; private set; }

        public Task<bool> IsCrazyfliePaired() =>
            gattService.IsCrazyfliePaired();

        /// <summary>
        /// This function starts the communication to the crazyflie commander
        /// to set flight setpoints based on the state of the flight controller
        /// </summary>
        public async Task<bool> Start()
        {
            // Set up the cancellation token 
            cancellationSource?.Dispose();
            this.PacketsSent = 0;

            var initialized = await gattService.Initialize();
            if (initialized)
            {
                cancellationSource = new CancellationTokenSource();
                this.backgroundTask = Task.Run(
                    () => CommanderSetpointThread(cancellationSource.Token));
            }
            return initialized;
        }

        /// <summary>
        /// Stops the communication to the crazyflie commander
        /// </summary>
        public async Task Stop()
        {
            cancellationSource?.Cancel();
            if (this.backgroundTask != null)
            {
                await this.backgroundTask;
                this.backgroundTask = null;
            }
            cancellationSource = null;
            // TODO: consider writing zeros here to shut motors off
            // Not critical, as the commander code in the CF FW will time out
        }

        /// <summary>
        /// Thread function for writing commander packets via BTH
        ///  Writes as fast as it can in a tight loop
        /// </summary>
        private async Task CommanderSetpointThread(CancellationToken cancellationToken)
        {
            // Write commander packets as fast as possible in a loop until cancelled 
            while (!cancellationToken.IsCancellationRequested)
            {
                var axes = await flightController.GetFlightControlAxes();
                var result = await gattService.WriteCommanderPacket(
                    (float)(axes.Roll * MaxPitchRollRate),
                    (float)(axes.Pitch * MaxPitchRollRate),
                    (float)(axes.Yaw * MaxYawRate),
                    (ushort)(axes.Thrust * MaxThrustPercent * MaxThrust));
                if (result)
                {
                    this.PacketsSent++;
                }
                else
                {
                    return;
                }
            }
        }
    }
}
