using System.Threading.Tasks;
using Windows.Foundation;

namespace CrazyflieClient
{
    /// <summary>
    /// Implementation of a flight controller for a UAP app.
    /// This implementation takes two Joystick objects with which 
    /// the roll/pitch/yaw/thrust axes are implemented
    ///
    /// TODO: Which axis controls which output should be configurable
    /// </summary>
    public class FlightController : IFlightController
    {
        private readonly IJoystick leftStick;
        private readonly IJoystick rightStick;

        public FlightController(IJoystick leftStick, IJoystick rightStick)
        {
            this.leftStick = leftStick;
            this.rightStick = rightStick;    
        }
        
        public async Task<FlightControlAxes> GetFlightControlAxes()
        {
            Point leftStickPos = await leftStick.GetJoystickPosition();
            Point rightStickPos = await rightStick.GetJoystickPosition();

            var axes = new FlightControlAxes(
                roll: rightStickPos.X,
                pitch: rightStickPos.Y,
                yaw: leftStickPos.X,
                thrust: leftStickPos.Y);
            return axes;
        }
    }
}
