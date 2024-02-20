using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Winners
{
    class Program
    {
        // Three valid race types 
        static HashSet<string> raceTypeSet = new HashSet<string> { "1000m", "eggRace", "sackRace" };

        static void Main()
        {
            try
            {
                string projectDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
                string filePath = Path.Combine(projectDirectory, "race-results.txt");
                List<Participant> participants = ReadRaceResults(filePath);
                List<Participant> winners = DetermineWinners(participants);
                Console.WriteLine(Environment.NewLine + $"{WinnerAnnouncement(winners)}");
                foreach (var winner in winners)
                {
                    Console.WriteLine($"{winner.Name} (ID: {winner.Id}) who had the average time of {winner.CalculateAverageTime()} seconds");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred: {e.Message}");
            }
        }
        #region Helpfunctions
        private static string WinnerAnnouncement(List<Participant> winners)
        {
            string winnerAnnouncement;

            if (winners.Count == 1)
            {
                winnerAnnouncement = "And the winner is:";
            }
            else
            {
                winnerAnnouncement = "And the winners are:";
            }
            return winnerAnnouncement;
        }

        // Read race results from file. 
        private static List<Participant> ReadRaceResults(string filePath)
        {
            List<Participant> participants = new List<Participant>();
            string[] lines = File.ReadAllLines(filePath);

            // Check every line in the txt file. 
            foreach (var line in lines)
            {
                // Split each line into an array of substrings. 
                string[] data = line.Split(',');

                // (1) First, check that each line contains the right amount of data.
                if (data.Length == 5)
                {
                    string name = data[0].Trim();

                    // (2) Check if participants name is valid 
                    if (!CheckName(name))
                    {
                        Console.WriteLine($"Invalid name for {name}: Participants name should only contain letters, hyphens and spaces.");
                        continue;
                    }

                    // (3) Try to parse the second element (id) of the data array as an 'int'. 
                    // The condition is true if the parsing is successful.
                    if (int.TryParse(data[1].Trim(), out int id))
                    {
                        DateTime startTime, endTime;

                        // (4) Check and convert substrings representing the start and end time into DateTime objects
                        // Expected format of substrings is "HH:mm:ss"
                        if (DateTime.TryParseExact(data[2].Trim(), "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime)
                            && DateTime.TryParseExact(data[3].Trim(), "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out endTime))
                        {
                            if( startTime <= endTime) 
                            {
                                string raceType = data[4].Trim();

                                // (5) Finally, check if race type is valid 
                                if (raceTypeSet.Contains(raceType))
                                {
                                    // Create temporary Participant object 
                                    Participant participant;

                                    // Check if an existing participant with the same ID but different name exists
                                    // FirstOrDefault will return the first element that matches id but not name.
                                    // If no element of this kind is found, return null.
                                    Participant existingParticipantWithDifferentName = participants.FirstOrDefault(p => p.Id == id && !string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));


                                    // Check if an existing participant with the same ID and different name was found.
                                    // If found, print an error message. 
                                    if (existingParticipantWithDifferentName != null)
                                    {
                                        // Example: Participant with ID 3878304 already exists with a different name: Julia Roberts vs Julia
                                        Console.WriteLine($"Error: Participant with ID {id} already exists with a different name: {existingParticipantWithDifferentName.Name} vs {name}");
                                        // Skip creating a new participant
                                        continue;
                                    }

                                    // Try to get the participant from the participants list
                                    // that matches the specified name (case--insensitive!) and id.
                                    // For example: 'Julia Roberts' and 'Julia ROBERTS' should be treated as identical.
                                    // For example: 'Julia Roberts' and 'Julia' (I assume) will not be treated as identical.
                                    Participant existingParticipant = participants.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase) && p.Id == id);

                                    // Check if an existing participant was found
                                    if (existingParticipant != null)
                                    {
                                        // If found, existingParticipant is assigned to the local variable participant. 
                                        // 'participant' holds a reference to the existing participant.
                                        // A change in the participant variable will affect the existing participant
                                        // since both variables point to the same object in memory.
                                        participant = existingParticipant;
                                    }
                                    else
                                    {
                                        // If not found, create a new participant with the specified name and id
                                        participant = new Participant(name, id);
                                    }
                                    // Add race result to participant 
                                    participant.Results.Add(new RaceResult(raceType, startTime, endTime));
                                    // If there is no existing participant, add participant to list of participants
                                    if (!participants.Contains(participant))
                                    {
                                        participants.Add(participant);
                                    }

                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(raceType))
                                    {
                                        raceType = "No race type added";
                                    }
                                    Console.WriteLine($"Invalid race type for {name}: {raceType} ");
                                }
                            }
                            else 
                            {
                                Console.WriteLine($"Invalid time format for {name}: startTime {data[2]} occurs later than endTime {data[3]}");
                            }
                        }
                            
                        else
                        {
                            Console.WriteLine($"Invalid time format for {name}: {data[2]} or {data[3]}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Invalid id format for {name}: {data[1]}");
                    }
                }
                else
                {
                    Console.WriteLine($"Invalid data format: {string.Join(",", data)}");
                }

            }
            Console.WriteLine(Environment.NewLine + "All valid participants (with at least one race):");
            foreach (var temp in participants)
            {
                Console.WriteLine($"{temp.Name} (ID: {temp.Id})");
            }
            return participants;
        }

        private static List<Participant> DetermineWinners(List<Participant> participants)
        {
            // Only participants who have taken part in all
            // three races have the chance to be the winner of 
            // the tournament
            List<Participant> participantsWithThreeRaces = participants
                .Where(p => p.Results.Count == 3)
                .ToList();
            Console.WriteLine(Environment.NewLine + $"Participants with three valid races:");
            foreach (var winner in participantsWithThreeRaces)
            {
                Console.WriteLine($"{winner.Name} (ID: {winner.Id}) who had the average time of {winner.CalculateAverageTime()} seconds");
            }
            // Find the minimum average time among participants with three races
            double minAverageTime = participantsWithThreeRaces
                .Select(q => q.CalculateAverageTime())
                .Min();

            // Filter out participants with average time equal to the minimum average
            // time among all participants who have taken part in all three races 
            List<Participant> winners = participantsWithThreeRaces
                .Where(p => p.CalculateAverageTime() == minAverageTime)
                .ToList();

            return winners;
        }

        private static bool CheckName(string name)
        {
            // Participants name should only contain letters, hyphens and/or spaces.
            return name.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-');
        }
        #endregion
    }
}

