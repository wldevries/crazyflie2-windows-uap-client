using System.Threading.Tasks;
using Windows.Foundation;


namespace CrazyflieClient
{
    //
    // Summary:
    //      Implementation of a flight controller for a UAP app.
    //      This implementation takes two Joystick objects with which 
    //      the roll/pitch/yaw/thrust axes are implemented
    //
    //      TODO: Which axis controls which output should be configurable
    public class FlightController : IFlightController
    {
        private IJoystick leftStick;
        private IJoystick rightStick;

        public FlightController(IJoystick leftStick, IJoystick rightStick)
        {
            this.leftStick = leftStick;
            this.rightStick = rightStick;    
        }

        //
        // IFlightController implementaiton
        //
        public async Task<FlightControlAxes> GetFlightControlAxes()
        {
            Point leftStickPos = await leftStick.GetJoystickPosition();
            Point rightStickPos = await rightStick.GetJoystickPosition();

            FlightControlAxes axes;
            axes.roll = rightStickPos.X;
            axes.pitch = rightStickPos.Y;
            axes.yaw = leftStickPos.X;
            axes.thrust = leftStickPos.Y;
            return axes;
        }
    }
}
