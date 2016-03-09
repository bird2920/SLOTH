using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sloth.Properties;

namespace Sloth
{
    public class Program
    {
        private static void Main(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            try
            {
                Console.WriteLine("Sloth File Organizer \r\n");
                Console.WriteLine("Enter extension pattern to search.");
                Console.WriteLine("  Leave blank for *.*");
                var pattern = Console.ReadLine();
                if (pattern == string.Empty)
                    pattern = Settings.Default.pattern;

                try
                {
                    string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                    string pathDownload = Path.Combine(pathUser, "Downloads");

                    Console.WriteLine("Enter input path");
                    Console.WriteLine("  Leave blank for Downloads folder");
                    var inputPath = Console.ReadLine();
                    if (inputPath != null && inputPath == string.Empty)
                        inputPath = pathDownload;
                    if (inputPath != null)
                    {
                        var info = new DirectoryInfo(inputPath);

                        Debug.Assert(pattern != null, "pattern != null");
                        var files = info.GetFiles(pattern).OrderBy(p => p.CreationTime).ToArray();

                        Console.WriteLine("Enter output path");
                        Console.WriteLine("  Leave blank for Downloads folder");

                        var outputPath = Console.ReadLine();
                        if (outputPath != null && outputPath == string.Empty)
                            outputPath = pathDownload + "\\" + "Sloth";

                        Console.Write("Press any key to start \r\n");
                        Console.ReadLine();

                        foreach (var file in files)
                        {
                            if (file.Extension == ".lnk") continue;
                            if (file.Extension == string.Empty) continue;
                            var directory = Directory.CreateDirectory(outputPath + "\\" + file.Extension.Substring(1));
                            Console.WriteLine(file.Name);
                            File.Move(file.FullName, directory.FullName + "\\" + file.Name);
                            //DirectoryInfo di = Directory.CreateDirectory(Properties.Settings.Default.OutputPath + "\\" + Convert.ToString(file.CreationTime.Year) + "\\" + Convert.ToString(file.CreationTime.Month));
                        }
                    }
                }
                catch (IOException exception)
                {

                    Console.WriteLine(exception.ToString());
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
                Console.ReadLine();
            }

            Console.WriteLine("");
            Console.WriteLine("Finished - Press enter to quit");
            Console.Read();
        }
    }
}
