using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProjectAssetReader;
using ProjectAssetReader.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: program.exe <rootFilePath> [<packageName> <packageVersion>]");
            return;
        }

        string rootFilePath;
        string packageName = null;
        string packageVersion = null;
        DiagnosticMode diagnosticMode;
        

        rootFilePath = args[0];
        
        diagnosticMode = (DiagnosticMode)(args.Length - 1);
        
        if (diagnosticMode != DiagnosticMode.FullDirectory)
        {
            packageName = args[1];
        }
        if (diagnosticMode == DiagnosticMode.SpecificPackage)
        {
            packageVersion = args[2];
        }

        var projectAssets = GenerateProjectAssets(rootFilePath);
        ProcessPackageDependencies(projectAssets, packageName, packageVersion, diagnosticMode);
    }

    private static void ProcessPackageDependencies(ConcurrentDictionary<string, ProjectAssetsConfiguration> projectAssets, 
        string packageName, string targetVersion, DiagnosticMode diagnosticMode)
    {
        switch (diagnosticMode)
        {
            case DiagnosticMode.FullDirectory:
                ExecuteFullDirectoryDiagnostic(projectAssets);
                break;
            case DiagnosticMode.FullPackage:
                ExecuteFullPackageDiagnostic(projectAssets, packageName);
                break;
            case DiagnosticMode.SpecificPackage:
                ProcessSpecificPackageVersion(projectAssets, packageName, targetVersion);
                break;
            default:
                break;
        }
    }

    private static DependencyTree CreateDependencyTree(ConcurrentDictionary<string, ProjectAssetsConfiguration> projectAssets)
    {
        var dependencyTree = new DependencyTree(projectAssets.Values.ToArray());
        dependencyTree.BuildTree();
        return dependencyTree;
    }

    private static void ExecuteFullDirectoryDiagnostic(ConcurrentDictionary<string, ProjectAssetsConfiguration> projectAssets)
    {
        char delimiter = '\t';
        var dependencyTree = CreateDependencyTree(projectAssets);
        ConsoleTable.SetHeaders($"Package Name{delimiter}Versions");
        foreach (var group in dependencyTree.AllPackages.Where(d => d.Name.StartsWith("Sdm", StringComparison.OrdinalIgnoreCase))
            .GroupBy(p => p.Name).OrderByDescending(d => d.ToArray().Length))
        {
            ConsoleTable.AddLine($"{group.Key}{delimiter}{group.Distinct().ToArray().Length}");
            //Console.WriteLine($"{group.Key}\t{group.Distinct().ToArray().Length}");
        }
        ConsoleTable.ShowOutput();
    }

    private static void ExecuteFullPackageDiagnostic(ConcurrentDictionary<string, ProjectAssetsConfiguration> projectAssets, string packageName)
    {
        var dependencyTree = CreateDependencyTree(projectAssets);
        //Find All required Package Versions
        var requiredVersions = dependencyTree.AllPackages.Where(p => p.Name.Equals(packageName)).Select(d => d.Version);
        //Print Full Report for each requirement (why do I need this package?)
        foreach (var requiredVersion in requiredVersions)
        {
            Console.WriteLine($"{packageName} - {requiredVersion}");
            var requiredPackage = dependencyTree.AllPackages.Single(p => p.Name.Equals(packageName) && p.Version.Equals(requiredVersion));
            foreach (var dependent in requiredPackage.Dependents)
            {
                Console.WriteLine($"\t{dependent.UniqueId}");
            }
        }
    }

    private static void ProcessSpecificPackageVersion(ConcurrentDictionary<string, ProjectAssetsConfiguration> projectAssets, string packageName, string targetVersion)
    {
        var dependencyTree = CreateDependencyTree(projectAssets);

        foreach (var package in dependencyTree.AllPackages.Where(p => p.Name.Equals(packageName) && CheckVersion(targetVersion, p.Version)))
        {
            Console.WriteLine($"{packageName} - {package.Version}");
            foreach (var dependent in package.Dependents)
            {
                Console.WriteLine($"\t{dependent.UniqueId}");
            }
        }
        
    }

    private static ConcurrentDictionary<string, ProjectAssetsConfiguration> GenerateProjectAssets(string rootFilePath)
    {
        var projectAssetsDict = new ConcurrentDictionary<string, ProjectAssetsConfiguration>();

        string[] projectAssetFiles = Directory.GetFiles(rootFilePath, "project.assets.json", SearchOption.AllDirectories);
        var settings = new JsonSerializerSettings() { MissingMemberHandling = MissingMemberHandling.Error, };
        foreach (string filePath in projectAssetFiles)
        {
            
            try
            {
                string jsonContent = File.ReadAllText(filePath);

                var projectConfiguration = JsonConvert.DeserializeObject<ProjectAssetsConfiguration>(jsonContent, settings);

                projectAssetsDict.TryAdd(projectConfiguration.Project.Restore.ProjectName, projectConfiguration);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file '{filePath}': {ex.Message}");
            }
        }

        return projectAssetsDict;
    }

    private static bool CheckVersion(string targetVersion, string version)
    {
        if (targetVersion.StartsWith('!'))
        {
            return targetVersion != $"!{version}";
        }
        return targetVersion == version;
    }
}
