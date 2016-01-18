using System;

namespace ConvertToTcx
{
    public class XtrainerDataPoint
    {
        public TimeSpan ElapsedTime { get; set; }
        public double SpeedKilometersPerHour { get; set; }
        public int PowerWatts { get; set; }
        public int CadenceRpm { get; set; }
        public int HeartRateBpm { get; set; }
        public double GradePercent { get; set; }
        public double DeltaAltitude
        {
            //get { return Math.Sin(GradePercent*Math.PI/180)*DeltaDistanceMeters; }
            get { return GradePercent/100*DeltaDistanceMeters; }
        }

        public double DeltaCalories { get; set; }
        public double DeltaDistanceMeters { get; set; }
        public bool IsLap { get; set; }
    }
}