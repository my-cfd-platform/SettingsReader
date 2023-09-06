namespace SettingsReader;

public class YamlPropertyAttribute : Attribute
{

    public YamlPropertyAttribute(string name = null)
    {
        Name = name;
    }
        
    public string Name { get; }
        
}


public class YamlAttributesOnlyAttribute : Attribute
{
        
}