using System.Reflection;
using System.Text;
using MyYamlSettingsParser;

namespace SettingsReader;

public static class YamlSerializer
{

    private static string CompileDotNotationKey(this YamlLine yamlLine)
    {
        var result = new StringBuilder();

        foreach (var key in yamlLine.Keys)
        {
            if (result.Length > 0)
                result.Append('.');

            result.Append(key);
        }

        return result.ToString();
    }


    private static Dictionary<string, List<YamlLine>> CompileKeys(this byte[] yaml)
    {
        var lines = new Dictionary<string, List<YamlLine>>();
            
        foreach (var yamlLine in yaml.ParseYaml())
        {
            var yamlKey = yamlLine.CompileDotNotationKey();
                
            if (!lines.ContainsKey(yamlKey))
                lines.Add(yamlKey, new List<YamlLine>());
            lines[yamlKey].Add(yamlLine);  
        }

        return lines;
    }
        
    public static T DeserializeFlatStructure<T>(this byte[] yaml) 
        where T: class, new()
    {
        var keys = yaml.CompileKeys();

        var type = typeof(T);
        var result = new T();
            
        foreach (var pi in type.GetProperties())
        {
            var attr = pi.GetCustomAttribute<YamlPropertyAttribute>();

            if (attr == null)
                continue;

            var propertyName = attr.Name ?? pi.Name;
                
            if (!keys.ContainsKey(propertyName))
                throw new Exception("Can not find yaml field: "+propertyName);

            var yamlData = keys[propertyName];

            if (yamlData.Count > 1)
            {
                foreach (var yamlLine in yamlData)
                    Console.WriteLine(propertyName+':'+yamlLine.Value);
                    
                throw new Exception("Several values at the field: "+propertyName);
            }

            try
            {
                var value = pi.ChangeType(yamlData[0].Value);
                pi.SetValue(result, value);

            }
            catch (Exception e)
            {
                Console.WriteLine("Can not parse property: "+propertyName);
                Console.WriteLine(e);
            }
                
        }

        return result;

    }
        
}