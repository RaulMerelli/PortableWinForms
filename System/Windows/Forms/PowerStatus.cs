﻿namespace System.Windows.Forms
{

    public enum PowerLineStatus
    {
        Offline = 0,
        Online = 1,
        Unknown = 255
    }

    [Flags]
    public enum BatteryChargeStatus
    {
        High = 1,
        Low = 2,
        Critical = 4,
        Charging = 8,
        NoSystemBattery = 128,
        Unknown = 255
    }

    public enum PowerState
    {
        Suspend = 0,
        Hibernate = 1
    }

    public class PowerStatus
    {
        private NativeMethods.SYSTEM_POWER_STATUS systemPowerStatus;

        internal PowerStatus()
        {
        }

        public PowerLineStatus PowerLineStatus
        {
            get
            {
                UpdateSystemPowerStatus();
                return (PowerLineStatus)systemPowerStatus.ACLineStatus;
            }
        }

        public BatteryChargeStatus BatteryChargeStatus
        {
            get
            {
                UpdateSystemPowerStatus();
                return (BatteryChargeStatus)systemPowerStatus.BatteryFlag;
            }
        }

        public int BatteryFullLifetime
        {
            get
            {
                UpdateSystemPowerStatus();
                return systemPowerStatus.BatteryFullLifeTime;
            }
        }

        public float BatteryLifePercent
        {
            get
            {
                UpdateSystemPowerStatus();
                float lifePercent = systemPowerStatus.BatteryLifePercent / 100f;
                return lifePercent > 1f ? 1f : lifePercent;
            }
        }

        public int BatteryLifeRemaining
        {
            get
            {
                UpdateSystemPowerStatus();
                return systemPowerStatus.BatteryLifeTime;
            }
        }

        private void UpdateSystemPowerStatus()
        {
            UnsafeNativeMethods.GetSystemPowerStatus(ref systemPowerStatus);
        }
    }
}