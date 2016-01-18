using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ConvertToTcx
{
    public class Converter
    {
        public void WriteTcxFile(IEnumerable<SourcedStream> laps, TextWriter textWriter)
        {
            using (var writer = new TcxWriter(textWriter))
            {
                writer.StartTcx();
                bool firstFile = true;
                var stats = new LapStats { Calories = 0, DistanceMeters = 0, TotalTimeSeconds = 0,AltitudeMeters = 0,AvgHeartRate = 0,MaxHeartRate = 0,MaxSpeed = 0};
                foreach (var lap in laps)
                {
                    var data = TcxDataFactory.CreateDefault().Create(lap);
                    if (firstFile)
                    {
                        writer.StartActivity(data.StartTime, data.Sport);
                        firstFile = false;
                    }

                    writer.StartLap(data.StartTime);

                    foreach (var point in data.TrackPoints)
                    {
                        if (point.IsLap)
                        {
                            double totalsec = writer.EndLap().TotalTimeSeconds;
                            stats = new LapStats { Calories = 0, DistanceMeters = 0, TotalTimeSeconds = totalsec, AltitudeMeters = 0, AvgHeartRate = 0, MaxHeartRate = 0, MaxSpeed = 0 };
                            writer.StartLap(data.StartTime.AddSeconds(stats.TotalTimeSeconds));
                        }
                        writer.StartTrackPoint();
                        writer.WriteTrackPointTime(point.Time);
                        writer.WriteTrackPointCadence(point.CadenceRpm);
                        writer.WriteTrackPointElapsedCalories(point.CaloriesElapsed + stats.Calories);
                        writer.WriteTrackPointElapsedDistanceMeters(point.DistanceMetersElapsed); // + stats.DistanceMeters);
                        writer.WriteTrackPointHeartRateBpm(point.HeartRateBpm);
                        writer.WriteTrackPointPowerWatts(point.PowerWatts);
                        writer.WriteTrackPointSpeedMetersPerSecond(point.SpeedMetersPerSecond);
                        writer.WriteTrackPointElapsedAltitude(point.AltitudeMetersElapsed + stats.AltitudeMeters);

                        writer.EndTrackPoint();
                    }

                    stats = writer.EndLap();
                }
                writer.EndActivity();
                writer.EndTcx();
            }
        }
    }
}
