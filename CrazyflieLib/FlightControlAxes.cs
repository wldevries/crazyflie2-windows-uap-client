namespace CrazyflieClient
{
    /// <summary>
    /// Structure describing the current setpoints
    //  of the flight control object.  This structure
    //  describes four axes: roll, pitch, yaw, thrust.
    //  Each value is a percentage multiplier corresponding
    //  to how far the axis has been set.  
    /// </summary>
    public class FlightControlAxes
    {
        /// <returns></returns>
        public FlightControlAxes(double roll, double pitch, double yaw, double thrust)
        {
            this.Roll = roll;
            this.Pitch = pitch;
            this.Yaw = yaw;
            this.Thrust = thrust;
        }

        // Value of the roll axis as a floating point
        // percentage in the range of (-1,1)
        // where -1 is full left and 1 is full right
        public double Roll { get; }

        // Value of the pitch axis as a floating point
        // percentage in the range of (-1,1)
        // where -1 is full backward and 1 is full forward
        public double Pitch { get; }

        // Value of the yaw axis as a floating point 
        // percentage in the range of (-1,1)
        // where -1 is full counter-clockwise and 1 is full clockwise
        public double Yaw { get; }

        // Value of the thrust axis as a floating point 
        // percentage in the range of (0,1) 
        // where 0 is no thrust and 1 is full thrust
        public double Thrust { get; }
    }
}
