using System.Reflection;
using DotNetCoreDecorators;
using Flurl.Http;
using MyYamlSettingsParser;

namespace SettingsReader;

internal static class SettingsReaderHelpers
{
    private const string SettingsUrlEnvVariable = "SETTINGS_URL";

    private static byte[] ReadingFromEnvVariable()
    {

        var settingsUrl = Environment.GetEnvironmentVariable(SettingsUrlEnvVariable);
        if (string.IsNullOrEmpty(settingsUrl))
        {
            Console.WriteLine($"Environment variable {SettingsUrlEnvVariable} is empty. Skipping reading settings from it");
            return null;
        }

        try
        {
            return settingsUrl.GetBytesAsync().Result;
        }
        catch (Exception e)
        {
            Console.WriteLine("Could not read settings from: "+settingsUrl+"; "+e.Message);
            return null;
        }
            
    }

    private static byte[] ReadFromFileInHome(string fileName)
    {
        var settingsFile = Environment.GetEnvironmentVariable("HOME").AddLastSymbolIfNotExists(Path.DirectorySeparatorChar) + fileName;

        try
        {
            return File.ReadAllBytes(settingsFile);
        }
        catch (Exception e)
        {
            Console.WriteLine("Could noy read settings from "+settingsFile+". "+e.Message);
            Console.WriteLine();
            return null;
        }
    }
        
    internal static T GetSettings<T>(string fileName = ".simple-trading") where T : class, new()
    {

        var type = typeof(T);

        if (type.GetCustomAttribute<YamlAttributesOnlyAttribute>() != null)
            return ReadFlatSettingsStructure<T>(fileName);

        var yaml = ReadFromFileInHome(fileName) ?? ReadingFromEnvVariable();

        if (yaml == null)
        {
            Console.WriteLine();
            throw new Exception("No settings found");
        }
            
        return yaml.ParseSettings<T>();
    }

        
    private static T ReadFlatSettingsStructure<T>(string fileName) where T : class, new()
    {
        var yaml = ReadFromFileInHome(fileName) ?? ReadingFromEnvVariable();

        if (yaml != null) 
            return yaml.DeserializeFlatStructure<T>();
            
        Console.WriteLine();
        throw new Exception("No settings found");
    }

}