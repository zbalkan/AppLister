using System;

namespace Engine.Factory
{
    internal partial class DriverFactory
    {
        public class DeviceDriverInfo
        {
            public string DeviceId { get; }

            public string DeviceName { get; }

            public DateTime DriverDate { get; }

            public string DriverInf { get; }

            public Version DriverVersion { get; }

            public bool? IsPresent { get; }

            public DeviceDriverInfo(string deviceId, string name, string inf, DateTime driverDate, Version driverVersion, bool? isPresent)
            {
                DeviceId = deviceId;
                DeviceName = name;
                DriverInf = inf;
                DriverDate = driverDate;
                DriverVersion = driverVersion;
                IsPresent = isPresent;
            }

            public override string ToString()
            {
                return $"Id: {DeviceId}, Name: {DeviceName}, Inf: {DriverInf}, DriverDate: {DriverDate}, DriverVersion: {DriverVersion}, Present: {IsPresent}";
            }
        }
    }
}