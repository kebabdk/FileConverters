using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ConvertToTcx
{
    public class LeMondDataReader
    {
        readonly ILeMondDataProvider _provider;

        public LeMondDataReader(ILeMondDataProvider provider)
        {
            _provider = provider;
        }

        public DateTime StartTime
        {
            get { return _provider.StartTime; }
        }

        public IEnumerable<LeMondDataPoint> DataPoints
        {
            get
            {
                foreach (var line in _provider.DataLines)
                {
                    yield return new LeMondDataPoint()
                                    {
                                        ElapsedTime = TimeSpan.Parse(line.Time),
                                        SpeedKilometersPerHour = _provider.ConvertSpeedToKilometersPerHour(double.Parse(line.Speed, CultureInfo.CreateSpecificCulture("en-US"))),
                                        DistanceKilometers = _provider.ConvertDistanceToKilometers(double.Parse(line.Distance, CultureInfo.CreateSpecificCulture("en-US"))),
                                        PowerWatts = int.Parse(line.Power),
                                        HeartRateBeatsPerMinute = int.Parse(line.HeartRate),
                                        CadenceRotationsPerMinute = int.Parse(line.Rpm),
                                        ElapsedCalories = int.Parse(line.Calories),
                                    };
                }
            }
        }
    }

}
