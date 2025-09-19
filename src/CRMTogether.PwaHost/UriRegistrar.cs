using Microsoft.Win32;
using System.Reflection;

namespace CRMTogether.PwaHost
{
    internal static class UriRegistrar
    {
        public static void RegisterUserProtocol(string scheme)
        {
            string exe = Assembly.GetExecutingAssembly().Location.Replace("\"","");
            using (var key = Registry.CurrentUser.CreateSubKey($@"Software\Classes\{scheme}"))
            {
                key.SetValue("", $"URL:{scheme} Protocol");
                key.SetValue("URL Protocol", "");
                using (var cmd = key.CreateSubKey(@"shell\open\command"))
                {
                    cmd.SetValue("", $"\"{exe}\" \"%1\"");
                }
            }
        }

        public static bool IsRegistered(string scheme)
        {
            try
            {
                using (var cmd = Registry.CurrentUser.OpenSubKey($@"Software\Classes\{scheme}\shell\open\command"))
                {
                    return cmd != null && cmd.GetValue(null) != null;
                }
            }
            catch { return false; }
        }
    }
}
