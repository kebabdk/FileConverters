using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertToTcx
{
    public class XtrainerTcxData : ITcxData
    {
        static readonly TimeSpan OneSecond = new TimeSpan(0, 0, 1);

        private readonly XtrainerDataReader _reader;
        public XtrainerTcxData(XtrainerDataReader reader)
        {
            _reader = reader;
        }

        public DateTime StartTime
        {
            get { return _reader.StartTime; }
        }

        public double CaloriesElapsed { get; set; }
        public double DistanceMetersElapsed { get; set; }
        public double AltitudeElapsed { get; set; }

        public TcxSport Sport
        {
            get { return TcxSport.Biking; }
        }

        public IEnumerable<TcxTrackPoint> TrackPoints
        {
            get 
            {
                bool firstPoint = true;
                foreach (var point in _reader.DataPoints)
                {
                    if (firstPoint)
                    {
                        // adding a fake first point becuase strava seems
                        // to like seeing seconds 0:00-1:00 for a minute instead of 0:01-1:00
                        // this new point will actually give us 61 points, but will be considerd
                        // a full minute
                        yield return CreateTrackPoint(point.ElapsedTime - OneSecond, point);
                        firstPoint = false;
                    }
                    yield return CreateTrackPoint(point.ElapsedTime, point);
                }

            }
        }

        private TcxTrackPoint CreateTrackPoint(TimeSpan effectiveElapsedTime, XtrainerDataPoint point)
        {
            CaloriesElapsed += point.DeltaCalories;
            DistanceMetersElapsed += point.DeltaDistanceMeters;
            AltitudeElapsed += point.DeltaAltitude;
            return new TcxTrackPoint()
            {
                Time = _reader.StartTime + effectiveElapsedTime,
                CadenceRpm = point.CadenceRpm,
                CaloriesElapsed = Convert.ToInt32(Math.Ceiling(CaloriesElapsed)),
                DistanceMetersElapsed = DistanceMetersElapsed,
                HeartRateBpm = point.HeartRateBpm,
                PowerWatts = point.PowerWatts,
                SpeedMetersPerSecond = ConvertTime.SecondsToHours(ConvertDistance.KilometersToMeters(point.SpeedKilometersPerHour)),
                AltitudeMetersElapsed = AltitudeElapsed,
                IsLap = point.IsLap
            };
        }
    }
}
