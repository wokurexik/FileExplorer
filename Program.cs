using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
// using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace FileExplorer
{
    internal class Program
    {
        static Stack<string> directoryStack = new Stack<string>(); 

        static void Main()
        {
            
            DriveInfo[] drives = DriveInfo.GetDrives();

            Console.WriteLine("Available Disks:");

            foreach (var drive in drives)
            {
                Console.WriteLine($"- {drive.Name}");
            }

            Console.Write("Choose a disk to start with: ");
            string selectedDisk = Console.ReadLine()?.ToUpper() ?? "C";
            string currentDirectory = $"{selectedDisk}:\\";
            // Check if the selected disk is valid
            if (drives.Any(drive => drive.Name.Equals($"{selectedDisk}:\\")))
            {
                // Set the starting directory based on the selected disk
                DisplayDirectoryContents(currentDirectory);
            }
            else
            {
                Console.WriteLine("Invalid disk selection.");
                return;
            }

           
            while (true)
            {
                Console.WriteLine("\nEnter a command: (help: see all commands)");
                Console.Write($"{currentDirectory}>>");
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                string[] command = SplitCommand(input)
                           .Select(arg => arg.Trim('"'))
                           .ToArray();

                string action = command[0].ToLower();

                switch (action)
                {
                    case "cd":
                        if (command.Length > 1)
                        {
                            
                            string newDirectory = Path.Combine(currentDirectory, command[1]);
                            if (Directory.Exists(newDirectory))
                            {
                                currentDirectory = newDirectory;
                                DisplayDirectoryContents(currentDirectory);
                            }
                            else
                            {
                                Console.WriteLine("Directory not found.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Please provide a directory.");
                        }
                        break;

                    case "code":
                        if (command.Length > 1)
                        {
                            string codeDirectory = Path.Combine(currentDirectory, command[1]);
                            if (Directory.Exists(codeDirectory))
                            {
                                OpenInVisualStudioCode(codeDirectory);
                            }
                            else
                            {
                                Console.WriteLine("VSC is not available.");
                            }
                        }
                        break;

                    case "tree":
                        DisplayDirectoryTree(currentDirectory, "");
                        break;

                    case "search":
                        if (command.Length > 1)
                        {
                            string searchTerm = command[1];
                            SearchFiles(currentDirectory, searchTerm);

                        }
                        else
                        {
                            Console.WriteLine("Please provide a search term for 'search' command.");
                        }
                        break;

                    case "searchdir":
                        if (command.Length > 1)
                        {
                            string searchDirTerm = command[1];
                            SearchDirectories(currentDirectory, searchDirTerm);
                            Console.WriteLine($"{currentDirectory}{searchDirTerm}");
                        }
                        else
                        {
                            Console.WriteLine("Please provide a search term for 'search' command.");
                        }
                        break;

                    case "delete":
                        if (command.Length > 1)
                        {
                            string deletePath = Path.Combine(currentDirectory, command[1]);

                            // Ask for confirmation
                            Console.Write($"Are you sure you want to delete '{deletePath}'? (yes/no): ");
                            string confirmation = Console.ReadLine()?.ToLower();

                            if (confirmation == "yes")
                            {
                                Delete(deletePath);
                            }
                            else
                            {
                                Console.WriteLine("Deletion canceled.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Please provide a file or folder to delete.");
                        }
                        break;

                    case "create":
                        if (command.Length > 1)
                        {
                            string createPath = Path.Combine(currentDirectory, command[1]);

                            if (command.Length > 2)
                            {
                                // Create both directory and file if specified
                                CreateDirectoryAndFile(createPath, command[2]);
                            }
                            else
                            {
                                // Determine whether to create a file or folder based on the presence of an extension
                                if (createPath.Contains('.'))
                                {
                                    // Create a file if it has an extension
                                    CreateFile(createPath);
                                }
                                else
                                {
                                    // Create a folder if it doesn't have an extension
                                    CreateFolder(createPath);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Please provide a file or folder name for 'create' command.");
                        }
                        break;

                    case "location":
                       Console.WriteLine(currentDirectory);
                        break;

                    case "contents":
                        DisplayDirectoryContents(currentDirectory); 
                        break;

                    case "open":
                        if (command.Length > 1)
                        {
                            string openPath = Path.Combine(currentDirectory, command[1]);
                            Open(openPath);
                        }
                        else if(command.Length == 1)
                        {
                            Open(currentDirectory);
                        }
                        else
                        {
                            Console.WriteLine("Please provide a file or folder name for 'open' command.");
                        }
                        break; ;

                    case "help":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(" ");
                        Console.WriteLine("Commands");
                        Console.WriteLine("----------------------------------------------------------------------------------------------------------");
                        Console.WriteLine("cd:            Go to different directory (ex. cd <directory>)");
                        Console.WriteLine(" ");
                        Console.WriteLine("code .:        Open the directory in VSC");
                        Console.WriteLine(" ");
                        Console.WriteLine("search:        Find files in your current directory (ex. search <directory>)");
                        Console.WriteLine(" ");
                        Console.WriteLine("searchdir:     Find foulder in your current directory (ex. searchdir <filename>)");
                        Console.WriteLine(" ");
                        Console.WriteLine("tree:          Create a directory tree structure in the current directory");
                        Console.WriteLine(" ");
                        Console.WriteLine("create:        Creates and opens file if the name contains '.' or foulder if it doesnt");
                        Console.WriteLine("               (ex. create <directory> , create <filename.txt>), create <directory> <filename.txt>");
                        Console.WriteLine(" ");
                        Console.WriteLine("delete:        Deletes curtain file/foulder (ex. delete <directory>, delete <filename.txt>)");
                        Console.WriteLine(" ");
                        Console.WriteLine("open:          Opens a foulder/file (ex. open <directory/filename>, open  --opens in current directory)");
                        Console.WriteLine(" ");
                        Console.WriteLine("front:         Move forward in steps");
                        Console.WriteLine(" ");
                        Console.WriteLine("contests:      Shows the contests of the directory");
                        Console.WriteLine(" ");
                        Console.WriteLine("back:          Move backward in steps");
                        Console.WriteLine(" ");
                        Console.WriteLine("location:      Current location");
                        Console.WriteLine(" ");
                        Console.WriteLine("exit:          Exit the app");
                        Console.WriteLine("-----------------------------------------------------------------------------------------------------------");
                        Console.ResetColor();

                        break;

                    case "back":
                        string parentDirectory = Directory.GetParent(currentDirectory)?.FullName;
                        if (parentDirectory != null)
                        {
                            directoryStack.Push(currentDirectory); // Push current directory onto the stack
                            currentDirectory = parentDirectory;
                            DisplayDirectoryContents(currentDirectory);
                        }
                        else
                        {
                            Console.WriteLine("Already at the root directory.");
                        }
                        break;

                    case "front":
                        if (directoryStack.Count > 0)
                        {
                            currentDirectory = directoryStack.Pop(); // Pop directory from the stack
                            DisplayDirectoryContents(currentDirectory);
                        }
                        else
                        {
                            Console.WriteLine("No directory to go forward to.");
                        }
                        break;

                    case "exit":
                        Environment.Exit(0);
                        break;

                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }

        }

        static void DisplayDirectoryContents(string path)
        {
            Console.WriteLine($"\nContents of {path}:");

            try
            {
                string[] directories = Directory.GetDirectories(path);
                string[] files = Directory.GetFiles(path);

                foreach (var directory in directories)
                {
                    Console.WriteLine($"[D] {Path.GetFileName(directory)}");
                }

                foreach (var file in files)
                {
                    Console.WriteLine($"[F] {Path.GetFileName(file)}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void OpenInVisualStudioCode(string directory)
        {
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "\"C:\\Users\\Vít Sláma\\AppData\\Local\\Programs\\Microsoft VS Code\\Code.exe\"",
                Arguments = $"-r \"{directory}\"",
                UseShellExecute = true,
                RedirectStandardOutput = false,
                CreateNoWindow = true
            };

            Process.Start(psi);
        }

        static void SearchFiles(string currentDirectory, string searchTerm)
        {
            try
            {
                var files = Directory.GetFiles(currentDirectory, $"*{searchTerm}*");
                if (files.Any())
                {
                    Console.WriteLine($"\nSearch results in {currentDirectory}:");
                    foreach (var file in files)
                    {
                        Console.WriteLine($"[F] {Path.GetFileName(file)}");
                    }
                }
                else
                {
                    Console.WriteLine($"No files found matching the search term in {currentDirectory}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

        static void SearchDirectories(string currentDirectory, string searchDirTerm)
        {
            try
            {
                var directories = Directory.GetDirectories(currentDirectory, $"*{searchDirTerm}*");
                if (directories.Any())
                {
                    Console.WriteLine($"\nSearch results in {currentDirectory} (Directories):");
                    foreach (var dir in directories)
                    {
                        Console.WriteLine($"[D] {Path.GetFileName(dir)}");
                    }

                    // Set the matched directory as the new current directory
                    currentDirectory = directories.First();
                }
                else
                {
                    Console.WriteLine($"No directories found matching the search term in {currentDirectory}.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        static void DisplayDirectoryTree(string path, string indent)
        {
            try
            {
                Console.WriteLine($"{indent}[D] {Path.GetFileName(path)}");

                // Display files in the current directory
                foreach (var file in Directory.GetFiles(path))
                {
                    Console.WriteLine($"{indent}  [F] {Path.GetFileName(file)}");
                }

                // Recursively display subdirectories
                foreach (var directory in Directory.GetDirectories(path))   
                {
                    DisplayDirectoryTree(directory, indent + "  ");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error displaying tree for '{path}': {ex.Message}");
            }
        }

        static void Delete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    Console.WriteLine($"File '{path}' deleted successfully.");
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true); // Recursively delete directories
                    Console.WriteLine($"Directory '{path}' deleted successfully.");
                }
                else
                {
                    Console.WriteLine($"Path '{path}' does not exist.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting '{path}': {ex.Message}");
            }
        }

        static void CreateFile(string path)
        {
            try
            {
                File.WriteAllText(path, string.Empty);
                Console.WriteLine($"File '{path}' created successfully.");
                Open(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating file '{path}': {ex.Message}");
            }
        }

        static void CreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                Console.WriteLine($"Folder '{path}' created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating folder '{path}': {ex.Message}");
            }
        }

        static void Open(string path)
        {
            try
            {
                Process.Start(path);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening '{path}': {ex.Message}");
            }
        }

        static void CreateDirectoryAndFile(string directoryPath, string fileName)
        {
            try
            {
                string fullPath = Path.Combine(directoryPath, fileName);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Console.WriteLine($"Folder '{directoryPath}' created successfully.");

                    // Open the folder using the default file explorer
                    Open(directoryPath);
                }

                // Create the file
                File.WriteAllText(fullPath, string.Empty);
                Console.WriteLine($"File '{fullPath}' created successfully.");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"Error creating directory and file '{directoryPath}': Access denied. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating directory and file '{directoryPath}': {ex.Message}");
            }
        }

        static IEnumerable<string> SplitCommand(string input)
        {
            bool insideQuotes = false;
            StringBuilder currentArg = new StringBuilder();
            List<string> args = new List<string>();

            foreach (char c in input)
            {
                if (c == '\"')
                {
                    insideQuotes = !insideQuotes;
                }
                else if (c == ' ' && !insideQuotes)
                {
                    if (currentArg.Length > 0)
                    {
                        args.Add(currentArg.ToString());
                        currentArg.Clear();
                    }
                }
                else
                {
                    currentArg.Append(c);
                }
            }

            if (currentArg.Length > 0)
            {
                args.Add(currentArg.ToString());
            }

            return args;
        }
    }
}
