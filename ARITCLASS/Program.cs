using System;
using System.Collections;
using System.Drawing;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace ARITCLASS
{
    public class CellInfo
    {
        public string shipId { get; set; }
        public int availableHit { get; set; }
    }

    public class Coordinate
    {
        public int x { get; set; }
        public int y { get; set; }
    }


    internal class Program
    {
        public static string[] getBattleshipOutput(string[] inputLines)
        {
            CellInfo[,] P1 = null;
            CellInfo[,] P2 = null;
            string currentShipID = null;
            string currentShipType = null;
            string[] currentShipId_Dimensions = null;
            int currentAvailableHitForP1 = 0;
            int currentAvailableHitForP2 = 0;

            Queue<string> missile_P1 = new Queue<string>();
            Queue<string> missile_P2 = new Queue<string>();

            bool isPlayerA1Chance = true;

            List<string> outputLines = new List<string>();

            foreach (string line in inputLines)
            {
                if (line.StartsWith("Enter area boundaries: "))
                {
                    string[] dimensions_MxN = line.Replace("Enter area boundaries: ", "").Split(" ");
                    int M = Convert.ToInt16(dimensions_MxN[0]);
                    int N = ((byte)(dimensions_MxN[1].ToCharArray()[0])) - 64;
                    P1 = new CellInfo[M, N];
                    P2 = new CellInfo[M, N];
                }
                else if (line.StartsWith("Type for battleship "))
                {
                    string[] shipId_Type = line.Replace("Type for battleship ", "").Split(": ");
                    currentShipID = shipId_Type[0];
                    currentShipType = shipId_Type[1];
                }
                else if (line.StartsWith("Dimension for battleship "))
                {
                    currentShipId_Dimensions = line.Replace("Dimension for battleship " + currentShipID + ": ", "").Split(" ");
                }
                else if (line.StartsWith("Location of battleship "))
                {
                    Regex regex = new Regex("^Location of battleship (.+) for player (.+): (.+)$");
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        string playerId = match.Groups[2].Value;
                        string shipLocation = match.Groups[3].Value;
                        int m = Convert.ToInt32(currentShipId_Dimensions[0]);
                        int n = Convert.ToInt32(currentShipId_Dimensions[1]);
                        if (playerId == "A")
                        {
                            List<Coordinate> coordinates = getCoordinates(shipLocation, m, n);
                            foreach (Coordinate coordinate in coordinates)
                            {
                                currentAvailableHitForP1 += ((currentShipType == "Q") ? 2 : 1);
                                P1[coordinate.x, coordinate.y] = new CellInfo()
                                {
                                    availableHit = (currentShipType == "Q") ? 2 : 1,
                                    shipId = currentShipID
                                };
                            }

                        }
                        else
                        {
                            List<Coordinate> coordinates = getCoordinates(shipLocation, m, n);
                            foreach (Coordinate coordinate in coordinates)
                            {
                                currentAvailableHitForP2 += ((currentShipType == "Q") ? 2 : 1);
                                P2[coordinate.x, coordinate.y] = new CellInfo()
                                {
                                    availableHit = (currentShipType == "Q") ? 2 : 1,
                                    shipId = currentShipID
                                };
                            }

                        }

                    }
                }
                else if (line.StartsWith("Missile targets for player "))
                {
                    Regex regex = new Regex("^Missile targets for player (.+): (.+)$");
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        if (match.Groups[1].Value == "A")
                        {
                            string[] missilesPos = match.Groups[2].Value.Split(" ");
                            foreach (string misile in missilesPos)
                            {
                                missile_P1.Enqueue(misile);
                            }
                        }
                        else
                        {
                            string[] missilesPos = match.Groups[2].Value.Split(" ");
                            foreach (string misile in missilesPos)
                            {
                                missile_P2.Enqueue(misile);
                            }
                        }

                    }
                }

            }

            while (true)
            {
                if (missile_P1.Count == 0 && missile_P2.Count == 0)
                {
                    outputLines.Add("Match Draw!");
                    break;
                }

                if (isPlayerA1Chance)
                {
                    if (currentAvailableHitForP2 == 0)
                    {
                        outputLines.Add("Player-1 won the battle");
                        break;
                    }

                    if (missile_P1.Count == 0)
                    {
                        outputLines.Add("Player-1 has no more missiles left");
                        isPlayerA1Chance = !isPlayerA1Chance;
                        continue;
                    }

                    string missilePos = missile_P1.Dequeue();
                    Coordinate missileCordinate = getCoordinate(missilePos);
                    CellInfo cellInfo = P2[missileCordinate.x, missileCordinate.y];
                    if (cellInfo == null || cellInfo.availableHit == 0)
                    {
                        isPlayerA1Chance = !isPlayerA1Chance;
                        outputLines.Add($"Player-1 fires a missile with target {missilePos} which missed");
                        continue;
                    }

                    if (cellInfo.availableHit > 0)
                    {
                        cellInfo.availableHit -= 1;
                        currentAvailableHitForP2 -= 1;
                        outputLines.Add($"Player-1 fires a missile with target {missilePos} which hit");
                        continue;

                    }
                }
                else
                {
                    if (currentAvailableHitForP1 == 0)
                    {
                        outputLines.Add("Player-2 won the battle");
                        break;
                    }
                    if (missile_P2.Count == 0)
                    {
                        outputLines.Add("Player-2 has no more missiles left");
                        isPlayerA1Chance = !isPlayerA1Chance;
                        continue;
                    }
                    string missilePos = missile_P2.Dequeue();
                    Coordinate missileCordinate = getCoordinate(missilePos);
                    CellInfo cellInfo = P1[missileCordinate.x, missileCordinate.y];
                    if (cellInfo == null || cellInfo.availableHit == 0)
                    {
                        isPlayerA1Chance = !isPlayerA1Chance;
                        outputLines.Add($"Player-2 fires a missile with target {missilePos} which missed");
                        continue;
                    }
                    if (cellInfo.availableHit > 0)
                    {
                        cellInfo.availableHit -= 1;
                        currentAvailableHitForP1 -= 1;
                        outputLines.Add($"Player-2 fires a missile with target {missilePos} which hit");
                        continue;
                    }
                }

            }

            return outputLines.ToArray();
        }

        public static Coordinate getCoordinate(string pos)
        {
            int y = pos[0] - 65;
            int x = Convert.ToInt16((pos.Substring(1))) - 1;
            return new Coordinate() { x = x, y = y };
        }

        public static List<Coordinate> getCoordinates(string pos, int m, int n)
        {
            int y = pos[0] - 65;
            int x = Convert.ToInt16((pos.Substring(1))) - 1;
            List<Coordinate> coordinates = new List<Coordinate>();

            for (int y_n = y; y_n <= n - 1 + y; y_n++)
            {
                for (int x_m = x; x_m <= m - 1 + x; x_m++)
                {
                    coordinates.Add(new Coordinate() { x = x_m, y = y_n });
                }
            }
            return coordinates;
        }
        static void Main(string[] args)
        {
            string[] inputStr = File.ReadAllLines("E://battleship_input.txt");
            string[] outputLines = getBattleshipOutput(inputStr);

            File.WriteAllLines("E://battleship_output.txt", outputLines);
            foreach (string line in outputLines)
            {
                Console.WriteLine(line);
            }

            return;













            //int currentLineIndex = 0;
            //int T = Convert.ToInt32(inputStr[0]);

            //List<string> outputlLines = new List<string>();
            //for (int i = 1; i <= T; i++)
            //{
            //    string[] inputs_XYZ = inputStr[++currentLineIndex].Split(" ");
            //    int x = Convert.ToInt32(inputs_XYZ[0]);
            //    int y = Convert.ToInt32(inputs_XYZ[1]);
            //    int z = Convert.ToInt32(inputs_XYZ[2]);


            //    outputlLines.Add((x + y == z || y + z == x || z + x == y) ? "YES" : "NO");
            //}
            //File.WriteAllLines("E://output.txt", outputlLines);
            //return;




            //int n = 6;
            //int countBit = 0;
            //for (int i = 1; i <= n; i++)
            //{
            //    BitArray bitArray = new BitArray(i);
            //    byte upto = (byte)Math.Ceiling(((double)Math.Log10(i)) / Math.Log10(2));
            //    for (byte b = 0; b <= upto; b++)
            //    {
            //        if (isBitSet(i, b))
            //        {
            //            countBit++;
            //        }
            //    }
            //}
            //Console.WriteLine(countBit);








            //// test = ((double)60 / 70)*100;
            ////Console.WriteLine(test);
            ////return;
            //Console.WriteLine("Hello, World!");
            //string fileName = @"E:\a.jpg";
            //byte[] fileData = File.ReadAllBytes(fileName);
            ////Console.WriteLine(isImageBW(fileData));
            //Console.WriteLine(getImageBWPercent(fileData));
            ////string pan = "AXGPT8163J";
            ////string panPattern = @"^[a-zA-Z]{5}[0-9]{4}[a-zA-Z]$";
            ////Regex regex = new Regex(panPattern);
            ////Match match = regex.Match(pan);
            ////if (match.Success)
            ////{

            ////    Console.WriteLine("valid pan");
            ////}
            ////else {
            ////    Console.WriteLine("invalid pan");
            ////}

            ////string test1 = "Hi your refno 4567 and OTP is 1234 on pan no AXGPT8163J";
            ////string test2 = "Hi your refno 2345 and OTP:1234 on pan no AXGPT8163J";
            ////string test3 = "Hi your refno 2266 and OTP in mod req is 1234 on pan no AXGPT8163J";
            ////string patternStr = @"OTP[^0-9]*([0-9]{4}).* ([a-zA-Z]{5}[0-9]{4}[a-zA-Z])$";
            ////Regex regex = new Regex(patternStr);
            ////Match match = regex.Match(test3);
            ////if (match.Groups.Count > 1)
            ////{
            ////    Console.WriteLine($"all match pattern group0:{match.Groups[0]}");
            ////    Console.WriteLine($"OTP group1:{match.Groups[1]}");
            ////    Console.WriteLine($"PAN group2:{match.Groups[2]}");
            ////}


            ////string test1 = "nileshtoonwal@gmail.com";
            ////string test2 = "Hi your refno 2345 and OTP:1234 on pan no AXGPT8163J";
            ////string test3 = "Hi your refno 2266 and OTP in mod req is 1234 on pan no AXGPT8163J";
            ////string patternStr = @"OTP[^0-9]*([0-9]{4}).* ([a-zA-Z]{5}[0-9]{4}[a-zA-Z])$";
            ////Regex regex = new Regex(patternStr);
            ////Match match = regex.Match(test1);
            ////if (match.Groups.Count > 1)
            ////{
            ////    Console.WriteLine($"all match pattern group0:{match.Groups[0]}");
            ////    Console.WriteLine($"OTP group1:{match.Groups[1]}");
            ////    Console.WriteLine($"PAN group2:{match.Groups[2]}");
            ////}

        }

        public static bool isImageBW(byte[] imageBytes)
        {
            int allowAcceptableError = 20;
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Bitmap img = new Bitmap(ms);
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        Color c = img.GetPixel(x, y);
                        if (Math.Abs(c.R - c.G) > allowAcceptableError
                            || Math.Abs(c.G - c.B) > allowAcceptableError
                            || Math.Abs(c.B - c.R) > allowAcceptableError)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public static double getImageBWPercent(byte[] imageBytes)
        {
            int allowAcceptableError = 20;

            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Bitmap img = new Bitmap(ms);
                int totalPixcels = img.Width * img.Height;
                int totalColoredPixcelFound = 0;
                for (int y = 0; y < img.Height; y++)
                {
                    for (int x = 0; x < img.Width; x++)
                    {
                        Color c = img.GetPixel(x, y);
                        if (Math.Abs(c.R - c.G) > allowAcceptableError
                            || Math.Abs(c.G - c.B) > allowAcceptableError
                            || Math.Abs(c.B - c.R) > allowAcceptableError)
                        {
                            totalColoredPixcelFound++;
                        }
                    }
                }
                return ((double)totalColoredPixcelFound / totalPixcels) * 100;
            }

        }

        public static bool isBitSet(int number, byte index)
        {
            return (number & (1 << index)) != 0;
        }
    }
}