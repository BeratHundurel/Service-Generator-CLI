using CommandLine;
using System;
using System.Diagnostics;
using System.IO;

public class Program
{
    public static readonly string servicePath  = Path.Combine(Directory.GetCurrentDirectory(), "service");
    public static readonly string interfacePath = Path.Combine(servicePath, "Abstract");
    public static readonly string concretePath = Path.Combine(servicePath, "Concrete", "EntityFramework");
    public static readonly string efUnitOfWorkPath = Path.Combine(concretePath, "EfUnitOfWork.cs");
    public static readonly string iUnitOfWorkPath = Path.Combine(interfacePath, "IUnitOfWork.cs");
    public class Options
    {
        [Value(0, MetaName = "action", Required = true, HelpText = "The action to perform, 'generate'")]
        public string Action { get; set; }

        [Value(1, MetaName = "name", Required = true, HelpText = "The name of the service to generate")]
        public string Name { get; set; }
    }

    /// <summary>
    /// The entry point of the program.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    public static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
               .WithParsed<Options>(opts => ExecuteCommand(opts.Action, opts.Name));
    }

    /// <summary>
    /// Executes the specified command with the given name.
    /// </summary>
    /// <param name="action">The action to perform. Currently supports 'generate'.</param>
    /// <param name="name">The name of the file to generate.</param>
    private static void ExecuteCommand(string action, string name)
    {
        if (action.ToLower() == "generate")
        {
            GenerateFiles(name);
            UpdateUnitOfWorkFile(name);
            FormatProject(servicePath);
        }
        else
        {
            Console.WriteLine($"Unknown action: {action}");
        }
    }

    /// <summary>
    /// Generates the necessary files for a specified service.
    /// </summary>
    /// <param name="command">The name of the service for which files are to be generated.</param>
    private static void GenerateFiles(string command)
    {
        try
        {
            string interfaceName = $"I{command}Service.cs";
            string className = $"Ef{command}Service.cs";

            Directory.CreateDirectory(interfacePath);
            Directory.CreateDirectory(concretePath);

            string interfaceContent =
                $@"using System;
            using entity.Models;
            namespace service.Abstract
            {{
                public interface I{command}Service : IGenericService<{command}>
                {{

                }};
            }}";

            string classContent =
                $@"using System;
            using entity.Context;
            using entity.Models;
            using Microsoft.AspNetCore.Http;
            using service.Abstract;
            using service.Concrete.EntityFramework;

            namespace service.Concrete.EntityFramework
            {{
                public class Ef{command}Service :EfGenericService<{command}>, I{command}Service
                {{
                    public readonly IHttpContextAccessor _httpContextAccessor;

                    public Ef{command}Service(DatabaseContext _context, IHttpContextAccessor httpContextAccessor) : base(_context)
                    {{
                        _httpContextAccessor = httpContextAccessor;
                    }} 

                    public DatabaseContext _db
                    {{
                        get {{ return _context as DatabaseContext; }}
                    }}
                }}
            }}";

            File.WriteAllText(Path.Combine(interfacePath, interfaceName), interfaceContent);
            File.WriteAllText(Path.Combine(concretePath, className), classContent);
            Console.WriteLine($"Files '{interfaceName}' and '{className}' have been created");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while creating service files: {ex.Message}");
        }
    }

    /// <param name="command">The name of the service for which to add a service field.</param>
    private static void UpdateUnitOfWorkFile(string command)
    {
        /// <summary>
        /// Updates the EfUnitOfWork.cs file to include a new service field for the specified service.
        /// </summary>
        try
        {
            string existingContent = File.ReadAllText(efUnitOfWorkPath);
            string serviceField = $"private I{command}Service _{command.ToLower()}Service;";
            string serviceGetter =
            $@"public I{command}Service {command}
            {{
                get
                {{
                    return _{command.ToLower()}Service ?? (_{command.ToLower()}Service = new Ef{command}Service(_db, _httpContextAccessor));
                }}
            }}";

            if (existingContent.Contains(serviceField) || existingContent.Contains(serviceGetter))
            {
                Console.WriteLine($"Service '{command}' dependency already exists in UnitOFWork");
                return;
            }

            int constructorIndex = existingContent.IndexOf("public EfUnitOfWork(");
            if (constructorIndex == -1)
            {
                Console.WriteLine("Error: Could not find constructor in EfUnitOfWork.cs.");
                return;
            }
            string updatedContent = existingContent.Insert(constructorIndex, $"\n\t\t{serviceField}\n");

            int constructorEndIndex = updatedContent.IndexOf('}', constructorIndex) + 1;
            if (constructorEndIndex == -1)
            {
                Console.WriteLine("Error: Could not find end of constructor in EfUnitOfWork.cs.");
                return;
            }

            updatedContent = updatedContent.Insert((constructorEndIndex), $"\n\n\t{serviceGetter}\n");
            File.WriteAllText(efUnitOfWorkPath, updatedContent);
            Console.WriteLine($"Updated EfUnitOfWork.cs to include service '{command}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating EfUnitOfWork: {ex.Message}");
        }

        /// <summary>
        /// Updates the IUnitOfWork.cs file to include a new service field for the specified service.
        /// </summary>
        try
        {
            string existingContent = File.ReadAllText(iUnitOfWorkPath);
            string serviceField = $"I{command}Service {command} {{ get; }}";

            if (existingContent.Contains(serviceField))
            {
                Console.WriteLine($"Service '{command}' already exists in IUnitOfWork.cs.");
                return;
            }

            int index = existingContent.IndexOf("IAdminService Admin { get; }");
            if (index == -1)
            {
                Console.WriteLine("Error: Could not find last service in IUnitOfWork.cs.");
                return;
            }

            string updatedContent = existingContent.Insert(index, $"\n\t\t{serviceField}\n");
            File.WriteAllText(iUnitOfWorkPath, updatedContent);
            Console.WriteLine($"Updated IUnitOfWork.cs to include service '{command}'.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while updating IUnitOfWork: {ex.Message}");
        }
    }

    /// <summary>
    /// Formats the project using the 'dotnet format' command.
    /// </summary>
    /// <param name="projectPath">The path of the project to format.</param>
    private static void FormatProject(string projectPath)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "format",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = projectPath
        };

        using (Process process = new Process { StartInfo = startInfo })
        {
            process.Start();
            process.WaitForExit();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            Console.WriteLine(output);
        }
    }
}