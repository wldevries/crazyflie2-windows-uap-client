using System.Threading.Tasks;
using Windows.Foundation;

namespace CrazyflieClient
{
    public interface IJoystick
    {
        Task<Point> GetJoystickPosition();
    }
}
