using System.Threading.Tasks;

namespace CrazyflieClient
{
    public interface IFlightController
    {
        /// <summary>
        /// Returns current flight control axes information
        /// All four values are returned as a synchronized 
        /// snapshot in time.
        /// </summary>
        Task<FlightControlAxes> GetFlightControlAxes();
    }
}
