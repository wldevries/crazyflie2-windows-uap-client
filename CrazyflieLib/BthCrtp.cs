﻿using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace CrazyflieClient
{
    // 
    // Summary:
    //      Class is an implementation of CRTP over BLE
    //      https://wiki.bitcraze.io/doc:crazyflie:ble:index?s[]=ble
    //      https://wiki.bitcraze.io/projects:crazyflie:crtp
    //
    class BthCrtp
    {
        //
        // Summary:
        //      Structure for a CRTP commander packet
        //      This will be sent OTA and must be byte packed
        //      https://wiki.bitcraze.io/projects:crazyflie:crtp:commander
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct CrtpControlPacket
        {
            public byte header;
            public float roll;
            public float pitch;
            public float yaw;
            public ushort thrust;

            public IBuffer ToBuffer()
            {
                using (var w = new DataWriter())
                {
                    w.WriteByte(header);
                    w.WriteSingle(roll);
                    w.WriteSingle(pitch);
                    w.WriteSingle(yaw);
                    w.WriteUInt16(thrust);
                    return w.DetachBuffer();
                }
            }
        }

        // This is the GUID for the CRTP GATT service
        private static Guid crtpServiceGuid =
            new Guid("00000201-1c7f-4f9e-947b-43b7c00a9a08");

        // This is the CRTP basic characteristic -- it is read/write
        // but only if the payload is less than or equal to 20 bytes
        private static Guid crtpCharacteristicGuid =
            new Guid("00000202-1c7f-4f9e-947b-43b7c00a9a08");

        // CRTP up characteristic -- used when payload is more than 20 bytes
        private static Guid crtpUpCharacteristicGuid =
            new Guid("00000203-1c7f-4f9e-947b-43b7c00a9a08");

        // CRTP down characteristic -- used when payload is more than 20 bytes
        private static Guid crtpDownCharacteristicGuid =
            new Guid("00000204-1c7f-4f9e-947b-43b7c00a9a08");

        private GattDeviceService crtpService;
        private GattCharacteristic crtpChar;
        private GattCharacteristic crtpUpChar;
        private GattCharacteristic crtpDownChar;

        public BthCrtp()
        {
            crtpService = null;
            crtpChar = null;
            crtpUpChar = null;
            crtpDownChar = null;
        }

        //
        // Summary:
        //      Checks enumerated devices for a device with the crazyflie service GUID.
        //      This function succeeds if the crazyflie is paired.  A return of 'true' 
        //      does not mean the device is connected or connectable.
        public async Task<Boolean> IsCrazyfliePaired()
        {
            var bthServices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(
                crtpServiceGuid), null);

            return bthServices.Count >= 1;
        }

        //
        // Summary: 
        //      Initializes all the CRTP service and characteristic objects
        //      Uses the first of each that's found
        public async Task<Boolean> InitCrtpService()
        {
            var bthServices = await Windows.Devices.Enumeration.DeviceInformation.FindAllAsync(
                GattDeviceService.GetDeviceSelectorFromUuid(
                crtpServiceGuid), null);

            // Use the first instance of this guid
            if (bthServices.Count >= 1)
            {
                crtpService = await GattDeviceService.FromIdAsync(bthServices[0].Id);
                if (crtpService != null)
                {
                    var chars = crtpService.GetCharacteristics(crtpCharacteristicGuid);
                    if (chars.Count >= 1)
                    {
                        crtpChar = chars[0];
                    }
                    var upChars = crtpService.GetCharacteristics(crtpUpCharacteristicGuid);
                    if (upChars.Count >= 1)
                    {
                        crtpUpChar = upChars[0];
                    }
                    var downChars = crtpService.GetCharacteristics(crtpDownCharacteristicGuid);
                    if (downChars.Count >= 1)
                    {
                        crtpDownChar = downChars[0];
                    }
                }
            }

            return ((crtpService != null) && (crtpChar != null) && (crtpUpChar != null) && (crtpDownChar != null));
        }

        // 
        // Summary:
        //      Writes a commander packet (roll, pitch, yaw, thrust) OTA via BLE
        public async Task WriteCommanderPacket(
            float roll,
            float pitch,
            float yaw,
            ushort thrust)
        {
            CrtpControlPacket packet = new CrtpControlPacket();

            // Header is always 0x30 for commander packets
            // (Port 3, link 0, chan 0)
            packet.header = 0x30;
            packet.roll = roll;
            packet.pitch = pitch;
            packet.yaw = yaw;
            packet.thrust = thrust;

            var buf = packet.ToBuffer();
                
            // Write the packet to the GATT characteristic
            // Can use the basic characteristic since the payload is always less than 20
            GattCommunicationStatus status = await crtpChar.WriteValueAsync(
                        buf,
                        GattWriteOption.WriteWithResponse);

            if (GattCommunicationStatus.Unreachable == status)
            {
                // TODO: error reporting
                return;
            }
            else
            {
                return;
            }
        }
    }
}
