using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Devices.Sensors.Metadata;
using Windows.Devices.Sensors.ObjectModel;

namespace Windows.Devices.Sensors
{
    [SensorDescription("ED4CA589-327A-4FF9-A560-91DA4B48275E")]
    internal class GPSInternal : Sensor
    {
        private static Guid SensorDataKey = new Guid("055C74D8-CA6F-47D6-95C6-1ED3637A0FF4");

        public GPSInternal() : base() { }

        public Location forceReloadLocation(){
            if (base.TryUpdateData())
            {
                return this.CurrentLocation;
            }
            return null;
        }       
        public Location CurrentLocation
        {
            get
            {
                if (this.DataReport == null && !base.TryUpdateData()){
                    Location location = new Location();
                    location.Latitude = 0;
                    location.Longitude = 0;
                    location.Accuracy = double.PositiveInfinity;
                    try
                    {
                        base.UpdateData();                        
                    }
                    catch (SensorPlatformException err)
                    {
                        location.NMEA = "err:" + err.InnerException.Message;
                    }
                    return location;
                }
                return GetLocation(this.DataReport);
            }
        }

        private Location GetLocation(SensorReport report)
        {
            if (report == null) { throw new ArgumentNullException("report"); }
            Location location = new Location();
            location.Timestamp = report.TimeStamp;
        /*
         * https://msdn.microsoft.com/en-us/library/windows/desktop/dd318981(v=vs.85).aspx
         * SENSOR_DATA_TYPE_GPS_STATUS (PID = 33) VT_I4 1=Valid, 2= not valid
         * SENSOR_DATA_TYPE_ERROR_RADIUS_METERS (PID = 22) VT_R8
         * SENSOR_DATA_TYPE_LATITUDE_DEGREES (PID = 2) VT_R8
         * SENSOR_DATA_TYPE_LONGITUDE_DEGREES (PID = 3) VT_R8
         * SENSOR_DATA_TYPE_SATELLITES_IN_VIEW (PID = 17) VT_I4
         * SENSOR_DATA_TYPE_SPEED_KNOTS (PID = 6) VT_R8
         * SENSOR_DATA_TYPE_NMEA_SENTENCE (PID = 38) 
         * 
         */
            location.NMEA = report.Values[SensorDataKey][25].ToString();
            try
            {
                location.Latitude = (double)report.Values[SensorDataKey][0];
                location.Longitude = (double)report.Values[SensorDataKey][1];
                location.Speed = 1.85 * (double)report.Values[SensorDataKey][5];
                location.Accuracy = (double)report.Values[SensorDataKey][2];
            }
            catch (InvalidCastException err)
            {
                location.NMEA = "error in cast to double";
            }
            try
            {
                location.Satelites = (Int32)report.Values[SensorDataKey][13];
            }
            catch (InvalidCastException err)
            {
                location.NMEA = "error in cast to int";
            }
            
#if DEBUG
            int cnt = report.Values[SensorDataKey].Count;
            String[] repValues = new String[cnt];
            for( int i=0;i<cnt; i++){
                var value = report.Values[SensorDataKey][i];

                repValues[i] = " " + i + "[" + value.GetType() + "]\t= " + value;
            }
            location.debugString = "[" + report.Values[SensorDataKey].Count + "]\n " +
                String.Join("\n", repValues);
            location.nativeReportValues = report.Values[SensorDataKey];
#endif
            return location;
        }

    }
}
