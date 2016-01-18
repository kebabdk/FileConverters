namespace ConvertToTcx
{
    public class XtrainerDataLine
    {
        public string Time { get; set; }
        public string HeartRate { get; set; }
        public string Rpm { get; set; }
        public string Power { get; set; }
        public string ClimbPromille { get; set; }
        public string Speed { get; set; }
        public bool IsLap { get; set; }

        //public string Distance { get; set; } //Calculated Speed/3,6  = 
        //public string Calories { get; set; } //calculated Watt*3,6 cal/h /3600s = watt/1000 
    }
}
