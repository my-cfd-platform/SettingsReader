namespace SettingsReader;

public static class SettingsReader
{
    private static object _value;

    private static Thread _thread;
        
    private static readonly object LockObject = new object();

    private static string _fileName;

    private static void ReadSettingsLoop<T>() where T : class, new()
    {

        while (_thread != null)
        {
            Thread.Sleep(30000);

            try
            {
                _value = SettingsReaderHelpers.GetSettings<T>(_fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Can not read settings in Background. " + ex.Message);
            }
        }

    }

    public static T ReadSettings<T>(string fileName = ".simple-trading") where T : class, new()
    {

        _fileName = fileName;

        lock (LockObject)
        {
            if (_value != null)
                return (T)_value;

            _value = SettingsReaderHelpers.GetSettings<T>(fileName);
            
            _thread = new Thread(ReadSettingsLoop<T>)
            {
                IsBackground = true,
                Name = "Settings reader in background"
            };
                
            _thread.Start();

            return (T)_value;
        }

    }
        
        
        
}