using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Sloth.Properties;

namespace Sloth
{
    public class Program
    {
        //int count;
        public static int NoInteract;

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                RunUserUI();

                Console.WriteLine("");
                Console.WriteLine("Finished - Press enter to quit");
                Console.Read();
            }
            else
            {
                var inputPath = args[0];
                var outputPath = args[1];
                var pattern = args[2];
                var type = args[3];
                var splitChar = args[4];
                var files = SearchFiles(pattern, inputPath);

                NoInteract = 1;

                Move(files, outputPath, type, splitChar);
            }
        }

        static void RunUserUI()
        {
            SlothHeader();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Enter extension pattern to search.");
            Console.ResetColor();
            Console.WriteLine("  Leave blank for *.*");
            var pattern = Console.ReadLine();
            if (pattern == string.Empty)
                //pattern = Settings.Default.pattern;
                pattern = "*.*";
            try
            {
                var pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var pathDownload = Path.Combine(pathUser, "Downloads");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Enter input path");
                Console.ResetColor();
                Console.WriteLine("  Leave blank for Downloads folder");

                UserInputManager(pattern, pathDownload);
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception.ToString());
                Console.ReadLine();
            }
        }

        static void SlothHeader()
        {
            Console.Title = "Sloth - The fastest way to organize files";
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(" SSSSS  LL       OOOOO  TTTTTTT HH   HH");
            Console.WriteLine("SS      LL      OO   OO   TTT   HH   HH B");
            Console.WriteLine(" SSSSS  LL      OO   OO   TTT   HHHHHHH I");
            Console.WriteLine("     SS LL      OO   OO   TTT   HH   HH R");
            Console.WriteLine(" SSSSS  LLLLLLL  OOOO0    TTT   HH   HH D");
            Console.ResetColor();
            Console.WriteLine("---------------------------------------");
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("Sloth File Organizer \r\n");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("To run without UI enter 'inPath, outPath, pattern, foldertype' in args");
            Console.ResetColor();
        }

        static void UserInputManager(string pattern, string pathDownload)
        {
            string splitCharacter = null;

            var inputPath = Console.ReadLine();
            if (inputPath != null && inputPath == string.Empty)
                inputPath = pathDownload;
            if (inputPath != null)
            {
                var files = SearchFiles(pattern, inputPath);

                //Output path
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Enter output path");
                Console.ResetColor();
                Console.WriteLine("  Leave blank for Downloads folder");
                var outputPath = OutPutPathValidate(pathDownload);

                //Folder Type (By Extension or Date)
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Enter number for folder type");
                Console.ResetColor();
                Console.WriteLine("  Select 1 for folders by Extension or leave blank");
                Console.WriteLine("  Select 2 for folders by Date");
                Console.WriteLine("  Select 3 for folders by Name");
                Console.WriteLine("  Select 4 for Move by LastAccessDate");

                var folderType = SetFolderOutputType();

                if (folderType == "3")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Enter character for filename split");
                    Console.ResetColor();
                    Console.WriteLine("  Everything to the left of this character");
                    Console.WriteLine("  will be used as the Parent Folder name");

                    splitCharacter = Console.ReadLine();

                    Move(files, outputPath, folderType, splitCharacter);
                }
                else if(folderType == "4")
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("  Enter year");
                    Console.ResetColor();
                    var Year = Console.ReadLine();

                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine("  Everything less than {0} will be moved to {1}", Year, outputPath);

                    Move(files, outputPath, folderType, Year);
                }
                else
                {
                    Move(files, outputPath, folderType, splitCharacter);
                }
            }
        }

        static void Move(FileInfo[] files, string outputPath, string folderType, string splitCharacter)
        {
            //Start File Move
            if(NoInteract == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("Press any key to start \r\n");
                Console.ResetColor();
                Console.ReadLine();
            }

            if (!Directory.Exists(outputPath))
            {
                outputPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads" + "\\" + "Sloth";
            }
            foreach (var file in files)
            {
                if (file.Extension == ".lnk") continue;
                if (file.Extension == string.Empty) continue;

                switch (folderType)
                {
                    case "1":
                        var directory = Directory.CreateDirectory(outputPath + "\\" + file.Extension.Substring(1));
                        if (!File.Exists(directory.FullName + "\\" + file.Name))
                        {
                            File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                        }
                        else
                        {
                            Console.WriteLine("File already exists:");
                            Console.WriteLine(file.FullName);
                        }
                        break;
                    case "2":
                        directory = Directory.CreateDirectory(outputPath + "\\" + Convert.ToString(file.CreationTime.Year) + "\\" + Convert.ToString(file.CreationTime.Month) + "\\" + "Day " + Convert.ToString(file.CreationTime.Day));
                        if (File.Exists(directory.FullName + "\\" + file.Name))
                        {
                            Console.WriteLine("File already exists");
                            Console.WriteLine(file.FullName);
                        }
                        else
                        {
                            File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                        }
                        break;
                    case "3":
                        if (!file.Name.Contains(splitCharacter))
                        {
                            directory = Directory.CreateDirectory(outputPath + "\\" + file.Extension.Substring(1));
                            File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                            Console.WriteLine(file.FullName);
                        }
                        else
                        {
                            var newFolderName = file.Name.Substring(0, file.Name.IndexOf(splitCharacter, StringComparison.CurrentCulture));
                            directory = Directory.CreateDirectory(outputPath + "\\" + newFolderName);
                            File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                            Console.WriteLine(file.FullName);
                        }
                        break;
                    case "4":
                        directory = Directory.CreateDirectory(outputPath + "\\" + Convert.ToString(file.CreationTime.Year) + "\\" + Convert.ToString(file.CreationTime.Month) + "\\" + "Day " + Convert.ToString(file.CreationTime.Day));
                        if (file.LastAccessTimeUtc.Year < Convert.ToInt32(splitCharacter))
                        {
                            if(File.Exists(directory.FullName + "\\" + file.Name))
                            {
                                Console.WriteLine("File already exists");
                            }
                            else
                            {
                                File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                                Console.WriteLine(file.FullName);
                            }
                        }
                        //else
                        //{
                        //    Console.WriteLine("Last access is > {0}", Convert.ToInt32(splitCharacter));
                        //}
                        break;
                    default:
                        directory = Directory.CreateDirectory(outputPath + "\\" + file.Extension.Substring(1));
                        File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                        break;
                }
            }
        }

        static string SetFolderOutputType()
        {
            var folderType = Console.ReadLine();
            if (folderType != null && folderType == string.Empty)
                folderType = "1";
            return folderType;
        }

        static string OutPutPathValidate(string pathDownload)
        {
            var outputPath = Console.ReadLine();

            if (outputPath != null && outputPath == string.Empty)
                outputPath = pathDownload + "\\" + "Sloth";
            return outputPath;
        }

        static FileInfo[] SearchFiles(string pattern, string inputPath)
        {
            if (!Directory.Exists(inputPath))
            {
                inputPath = Environment.GetEnvironmentVariable("USERPROFILE") + @"\" + "Downloads";
            }

            var info = new DirectoryInfo(inputPath);

            Debug.Assert(pattern != null, "pattern != null");
            var files = info.GetFiles(pattern).OrderBy(p => p.CreationTime).ToArray();
            return files;
        }

    }
}
