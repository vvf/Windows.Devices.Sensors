using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Sensors.Metadata;
using Windows.Devices.Sensors.ObjectModel;

namespace Windows.Devices.Sensors
{
    public class GPS
    {
        public static GPS GetDefault()
        {
            try
            {
                var sensorList = SensorManager.GetSensorsByTypeId<GPSInternal>();
                if (sensorList.Count > 0)
                {
                    return new GPS(sensorList[0]);
                }
            }
            catch
            {
            }
            return null;
        }

        private Sensors.GPSInternal m_gps;

		private GPS(Sensors.GPSInternal gps)
		{
			m_gps = gps;
			m_gps.DataReportChanged += DataReportChanged;
            m_gps.StateChanged += onStateChanged;
		}
        private void onStateChanged(Sensor sender, EventArgs e)
        {
            if (StateChanged != null)
            {
                StateChanged(this, EventArgs.Empty);
            }
        }
		private void DataReportChanged(Sensor sender, EventArgs e)
		{
			if (ReadingChanged != null)
			{
                var location = m_gps.CurrentLocation;
				ReadingChanged(this, new GPSReadingChangedEventArgs(
                    new GPSReading(location)));
			}
		}
		
        public event EventHandler<GPSReadingChangedEventArgs> ReadingChanged;
        public event EventHandler<EventArgs> StateChanged;

		public GPSReading GetCurrentReading()
		{
			var location = m_gps.CurrentLocation;
            return new GPSReading(location);
		}

		public uint MinimumReportInterval 
		{
			get { return m_gps.MinimumReportInterval; }
		}

		public uint ReportInterval
		{
			get { return m_gps.ReportInterval; }
			set { m_gps.ReportInterval = value; }
		}
        public String State
        {
            get { return m_gps.State.ToString(); }
        }

        public bool tryToReloadLocation()
        {
            return null != m_gps.forceReloadLocation();
        }
    }

    public sealed class GPSReadingChangedEventArgs : EventArgs
	{
		public GPSReadingChangedEventArgs(GPSReading reading)
		{
			Reading = reading;
		}
		public GPSReading Reading { get; private set; }
	}

	public sealed class GPSReading
	{
        internal GPSReading(Location _location)
		{
            location = _location;
		}
        public Location location { get; private set; }
	}
    public class Location
    {
        public double Latitude;
        public double Longitude;
        public double Accuracy;
        public DateTimeOffset Timestamp;
        public String NMEA;
        public Object nativeReportValues;
        public string debugString;
        public double Speed;
        public int Satelites;
        public override String ToString()
        {
#if DEBUG
            return debugString;
#else
            return "Location (" + Latitude + ", " + Longitude + ") ~" + Accuracy;
#endif
        }
    }
}

