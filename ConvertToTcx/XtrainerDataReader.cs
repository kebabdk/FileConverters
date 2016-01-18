using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertToTcx
{
    public class XtrainerDataReader
    {
        readonly IXtrainerDataProvider _provider;

        public XtrainerDataReader(IXtrainerDataProvider provider)
        {
            _provider = provider;
        }

        public DateTime StartTime
        {
            get { return _provider.StartTime; }
        }

        public IEnumerable<XtrainerDataPoint> DataPoints
        {
            get
            {
                foreach (var line in _provider.DataLines)
                {
                    
                    yield return new XtrainerDataPoint
                                    {
                                        ElapsedTime =new TimeSpan(0, 0, int.Parse(line.Time)),
                                        SpeedKilometersPerHour = double.Parse(line.Speed),
                                        PowerWatts = int.Parse(line.Power),
                                        HeartRateBpm= int.Parse(line.HeartRate),
                                        CadenceRpm = int.Parse(line.Rpm),
                                        GradePercent = double.Parse(line.ClimbPromille)/10,

                                        DeltaCalories = double.Parse(line.Power)/1000,
                                        DeltaDistanceMeters = double.Parse(line.Speed)/3.6,
                                        IsLap = line.IsLap
                                    };
                }
            }
        }
    }

}
