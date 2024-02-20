using System;
using System.Collections.Generic;
using System.Linq;
using System.Text; 

namespace Winners
{
    class Participant
    {
        public Participant(string name, int id)
        {
            Name = name;
            Id = id;
            Results = new List<RaceResult>();
        }

        public void AddResult(RaceResult result)
        {
            Results.Add(result);
        }

        public string Name
        {
            get;
            set;
        }
        public int Id
        {
            get;
            set;
        }
        public List<RaceResult> Results
        {
            get;
            set;
        }

        public void PrinResults()
        {
            foreach (var p in Results)
            {
                Console.WriteLine($"{p.RaceType}, {p.StartTime.ToString("HH:mm:ss")}, {p.EndTime.ToString("HH:mm:ss")}, ");
            }
        }

        public double CalculateAverageTime()
        {
            double totalTime = Results.Sum(result => result.GetTimeInSeconds());
            // Round to 1 decimal 
            return Math.Round(totalTime / Results.Count, 1, MidpointRounding.AwayFromZero);
        }
    }
}
