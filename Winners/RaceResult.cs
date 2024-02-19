using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winners
{
    class RaceResult
    {
        public RaceResult(string raceType, DateTime startTime, DateTime endTime)
        {
            RaceType = raceType;
            StartTime = startTime;
            EndTime = endTime;
        }
        public string RaceType
        {
            get;
            set;
        }
        public DateTime StartTime
        {
            get;
            set;
        }
        public DateTime EndTime
        {
            get;
            set;
        }

        public double GetTimeInSeconds()
        {   // Return total amount of seconds 
            return ((EndTime - StartTime).TotalSeconds);
        }
    }
}
