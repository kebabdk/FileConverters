using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace ConvertToTcx
{
    public enum TcxSport
    {
        Biking,
        Running,
        Other,
    }

    public class LapStats
    {
        public double TotalTimeSeconds { get; set; }
        public double DistanceMeters { get; set; }
        public double AltitudeMeters { get; set; }
        public int Calories { get; set; }
        public int AvgHeartRate { get; set; }
        public int MaxHeartRate { get; set; }
        public double MaxSpeed { get; set; }

    }

    public class LapPoint
    {
        public DateTime? Time { get; set; }
        public double? ElapsedDistanceMeters { get; set; }
        public int? HeartRateBpm { get; set; }
        public int? Cadence { get; set; }
        public double? SpeedMetersPerSecond { get; set; }
        public int? PowerWatts { get; set; }
        public int? ElapsedCalories { get; set; }
        public double? ElapsedAltitudeMeters { get; set; }
    }


    public class TcxWriter : IDisposable
    {
        private const string TcxV2XmlNamespace = "http://www.garmin.com/xmlschemas/TrainingCenterDatabase/v2";
        private const string ActivityExtensionsV2XmlNamespace = "http://www.garmin.com/xmlschemas/ActivityExtension/v2";

        private bool _tcxStarted;
        private XmlWriter _xmlWriter;
        private bool _inActivity;
        private List<LapPoint> _lapPoints;

        public TcxWriter(TextWriter textWriter)
        {
            _xmlWriter = XmlWriter.Create(textWriter);
        }

        public void StartTcx()
        {
            if(_tcxStarted)
            {
                throw new InvalidOperationException("Can't start the TCX twice");
            }
            _tcxStarted = true;

            _xmlWriter.WriteStartElement("TrainingCenterDatabase", TcxV2XmlNamespace);
            _xmlWriter.WriteStartElement("Activities", TcxV2XmlNamespace);

        }

        public void EndTcx()
        {
            if(!_tcxStarted)
            {
                throw new InvalidOperationException("Can't end a TCX that wasn't started");
            }

            // </Activities>
            _xmlWriter.WriteEndElement();
            // </TrainingCenterDatabase>
            _xmlWriter.WriteEndElement();
        }

        public void StartActivity(DateTime startTime, TcxSport sport)
        {
            if(!_tcxStarted)
            {
                StartTcx();
            }

            if(_inActivity)
            {
                EndActivity();
            }
            _inActivity = true;

            _xmlWriter.WriteStartElement("Activity", TcxV2XmlNamespace);
            _xmlWriter.WriteAttributeString("Sport", sport.ToString());

            // write the Id
            _xmlWriter.WriteStartElement("Id", TcxV2XmlNamespace);
            _xmlWriter.WriteValue(ConvertDateTime(startTime));
            _xmlWriter.WriteEndElement();
        }

        public void EndActivity()
        {
            _inActivity = false;
            
            // </Activity>
            _xmlWriter.WriteEndElement();
        }

        public void StartLap(DateTime startTime)
        {
            if (_lapPoints != null)
            {
                throw new InvalidOperationException("Can't start a lap before calling EndLap on the previous lap");
            }

            _xmlWriter.WriteStartElement("Lap", TcxV2XmlNamespace);
            
            // StartTime attribute
            _xmlWriter.WriteStartAttribute("StartTime");
            _xmlWriter.WriteValue(ConvertDateTime(startTime));
            _xmlWriter.WriteEndAttribute();

            _lapPoints = new List<LapPoint>();
        }

        public LapStats EndLap()
        {
            LapStats stats = new LapStats();

            if (_lapPoints.Count > 0)
            {
                // collect the stats from the points
                stats.TotalTimeSeconds = (_lapPoints.Last().Time.Value - _lapPoints.First().Time.Value).TotalSeconds;
                stats.DistanceMeters = _lapPoints.Max(p => p.ElapsedDistanceMeters??0);
                stats.Calories = _lapPoints.Last().ElapsedCalories??0;
                stats.AvgHeartRate = Convert.ToInt32(_lapPoints.Average(p => p.HeartRateBpm??0));
                stats.MaxHeartRate = Convert.ToInt32(_lapPoints.Max(p => p.HeartRateBpm ?? 0));
                stats.MaxSpeed = Convert.ToInt32(_lapPoints.Max(p => p.SpeedMetersPerSecond ?? 0));
                
            }

            // write out the status before we write out the track points
            // as required by the schema
            WriteElementAndValue("TotalTimeSeconds", TcxV2XmlNamespace, stats.TotalTimeSeconds);
            WriteElementAndValue("DistanceMeters", TcxV2XmlNamespace, stats.DistanceMeters);
            WriteElementAndValue("Calories", TcxV2XmlNamespace, stats.Calories);
            WriteElementAndValue("AverageHeartRateBpm", TcxV2XmlNamespace, stats.AvgHeartRate);
            WriteElementAndValue("MaximumHeartRateBpm", TcxV2XmlNamespace, stats.MaxHeartRate);
            WriteElementAndValue("MaximumSpeed", TcxV2XmlNamespace, stats.MaxSpeed);

            WriteElementAndValue("Intensity", TcxV2XmlNamespace, "Active");
            WriteElementAndValue("TriggerMethod", TcxV2XmlNamespace, "Manual");

            if (_lapPoints.Count > 0)
            {
                // write out each of the track points
                _xmlWriter.WriteStartElement("Track", TcxV2XmlNamespace);
                foreach (var point in _lapPoints)
                {
                    WriteTrackPoint(point);
                }
                _xmlWriter.WriteEndElement();
            }

            // </Lap>
            _xmlWriter.WriteEndElement();

            _lapPoints = null;
            return stats;
        }

        private void WriteTrackPoint(LapPoint point)
        {
            _xmlWriter.WriteStartElement("Trackpoint", TcxV2XmlNamespace);
            
            WriteElementAndValue("Time", TcxV2XmlNamespace, ConvertDateTime(point.Time.Value));
            WriteElementAndValue("DistanceMeters", TcxV2XmlNamespace, point.ElapsedDistanceMeters.Value);
            WriteElementAndValueElement("HeartRateBpm", TcxV2XmlNamespace, point.HeartRateBpm.Value);
            WriteElementAndValue("Cadence", TcxV2XmlNamespace, point.Cadence.Value);
            WriteElementAndValue("AltitudeMeters", TcxV2XmlNamespace, point.ElapsedAltitudeMeters.Value);

            // Extensions
            _xmlWriter.WriteStartElement("Extensions", TcxV2XmlNamespace);
            _xmlWriter.WriteStartElement("TPX", ActivityExtensionsV2XmlNamespace);

            WriteElementAndValue("Speed", ActivityExtensionsV2XmlNamespace, point.SpeedMetersPerSecond.Value);
            WriteElementAndValue("Watts", ActivityExtensionsV2XmlNamespace, point.PowerWatts.Value);
            
            // </TPX>
            _xmlWriter.WriteEndElement();
            // </Extensions>
            _xmlWriter.WriteEndElement();
            
            // </TrackPoint>
            _xmlWriter.WriteEndElement();
        }

        private object ConvertDateTime(DateTime dateTime)
        {
            return dateTime.ToUniversalTime();
        }

        private void WriteElementAndValue(string localName, string xmlNamespace, object value)
        {
            _xmlWriter.WriteStartElement(localName, xmlNamespace);
            _xmlWriter.WriteValue(value);
            _xmlWriter.WriteEndElement();
        }

        private void WriteElementAndValueElement(string localName, string xmlNamespace, object value)
        {
            _xmlWriter.WriteStartElement(localName, xmlNamespace);
            WriteValueElement(value);
            _xmlWriter.WriteEndElement();
        }

        private void WriteValueElement(object value)
        {
            _xmlWriter.WriteStartElement("Value", TcxV2XmlNamespace);
            _xmlWriter.WriteValue(value);
            _xmlWriter.WriteEndElement();
        }

        public void StartTrackPoint()
        {
            _lapPoints.Add(new LapPoint());
        }

        public void WriteTrackPointTime(DateTime time)
        {
            _lapPoints.Last().Time = time;
        }

        public void WriteTrackPointElapsedDistanceMeters(double elapsedDistanceMeters)
        {
            _lapPoints.Last().ElapsedDistanceMeters = elapsedDistanceMeters;
        }

        public void WriteTrackPointHeartRateBpm(int heartRateBpm)
        {
            _lapPoints.Last().HeartRateBpm = heartRateBpm;
        }

        public void WriteTrackPointCadence(int cadence)
        {
            _lapPoints.Last().Cadence = cadence;
        }

        public void WriteTrackPointSpeedMetersPerSecond(double speedMetersPerSecond)
        {
            _lapPoints.Last().SpeedMetersPerSecond = speedMetersPerSecond;
        }

        public void WriteTrackPointPowerWatts(int powerWatts)
        {
            _lapPoints.Last().PowerWatts = powerWatts;
        }

        public void WriteTrackPointElapsedCalories(int elapsedCalories)
        {
            _lapPoints.Last().ElapsedCalories = elapsedCalories;
        }

        public void WriteTrackPointElapsedAltitude(double elapsedAltitude)
        {
            _lapPoints.Last().ElapsedAltitudeMeters = elapsedAltitude;
        }

        public void EndTrackPoint()
        {
            // NoOp for now
        }



        public void Dispose()
        {
            if (_xmlWriter != null)
            {
                ((IDisposable)_xmlWriter).Dispose();
                _xmlWriter = null;
            }
        }


    }


}
