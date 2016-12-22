using System;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace CrazyflieClient
{
    /// <summary>
    /// Implementation of the CRTP service over BLE
    /// https://wiki.bitcraze.io/doc:crazyflie:ble:index?s[]=ble
    /// https://wiki.bitcraze.io/projects:crazyflie:crtp
    /// </summary>
    public class CrtpGattService
    {
        /// <summary>
        /// CRTP GATT service
        /// </summary>
        private readonly static Guid ServiceGuid =
            new Guid("00000201-1c7f-4f9e-947b-43b7c00a9a08");

        /// <summary>
        /// CRTP basic characteristic -- it is read/write
        /// but only if the payload is less than or equal to 20 bytes
        /// </summary>
        private readonly static Guid CharacteristicGuid =
            new Guid("00000202-1c7f-4f9e-947b-43b7c00a9a08");

        /// <summary>
        /// CRTP up characteristic -- used when payload is more than 20 bytes
        /// </summary>
        private readonly static Guid UpCharacteristicGuid =
            new Guid("00000203-1c7f-4f9e-947b-43b7c00a9a08");

        /// <summary>
        /// CRTP down characteristic -- used when payload is more than 20 bytes
        /// </summary>
        private readonly static Guid DownCharacteristicGuid =
            new Guid("00000204-1c7f-4f9e-947b-43b7c00a9a08");

        private GattDeviceService crtpService;
        private GattCharacteristic crtpChar;
        private GattCharacteristic crtpUpChar;
        private GattCharacteristic crtpDownChar;

        public CrtpGattService()
        {
            crtpService = null;
            crtpChar = null;
            crtpUpChar = null;
            crtpDownChar = null;
        }

        /// <summary>
        /// Checks enumerated devices for a device with the crazyflie service GUID.
        /// This function succeeds if the crazyflie is paired.  A return of 'true' 
        /// does not mean the device is connected or connectable.
        /// </summary>
        public async Task<Boolean> IsCrazyfliePaired()
        {
            var bthServices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(
                ServiceGuid), null);

            return bthServices.Count >= 1;
        }

        /// <summary>
        /// Initializes all the CRTP service and characteristic objects
        //  Uses the first of each that's found
        /// </summary>
        public async Task<bool> Initialize()
        {
            var bthServices = await DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(
                ServiceGuid), null);

            // Use the first instance of this guid
            if (bthServices.Count >= 1)
            {
                crtpService = await GattDeviceService.FromIdAsync(bthServices[0].Id);
                if (crtpService != null)
                {
                    var chars = crtpService.GetCharacteristics(CharacteristicGuid);
                    if (chars.Count >= 1)
                    {
                        crtpChar = chars[0];
                    }
                    var upChars = crtpService.GetCharacteristics(UpCharacteristicGuid);
                    if (upChars.Count >= 1)
                    {
                        crtpUpChar = upChars[0];
                    }
                    var downChars = crtpService.GetCharacteristics(DownCharacteristicGuid);
                    if (downChars.Count >= 1)
                    {
                        crtpDownChar = downChars[0];
                    }
                }
            }

            return ((crtpService != null) && (crtpChar != null) && (crtpUpChar != null) && (crtpDownChar != null));
        }

        /// <summary>
        /// Write a commander packet (roll, pitch, yaw, thrust) OTA via BLE
        /// </summary>
        public async Task<bool> WriteCommanderPacket(
            float roll,
            float pitch,
            float yaw,
            ushort thrust)
        {
            if (this.crtpService == null)
            {
                return false;
            }

            IBuffer buf;
            using (var w = new DataWriter())
            {
                // Header is always 0x30 for commander packets
                // (Port 3, link 0, chan 0)
                w.WriteByte(0x30);
                w.WriteSingle(roll);
                w.WriteSingle(pitch);
                w.WriteSingle(yaw);
                w.WriteUInt16(thrust);
                buf = w.DetachBuffer();
            }

            // Write the packet to the GATT characteristic
            // Can use the basic characteristic since the payload is always less than 20
            var status = await crtpChar.WriteValueAsync(
                        buf,
                        GattWriteOption.WriteWithResponse);

            return status == GattCommunicationStatus.Success;
        }
    }
}
