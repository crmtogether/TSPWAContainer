using System;
using System.IO;
using System.Web;

namespace CRMTogether.PwaHost
{
    internal static class UriCommandDispatcher
    {
        
        public static void Dispatch(string uri, MainForm form)
        {
            var u = uri.Replace("crmtog://", "crmtog:").Substring("crmtog:".Length);
            if (u.StartsWith("//")) u = u.Substring(2);
            string path = u;
            string query = "";
            var qmark = u.IndexOf('?');
            if (qmark >= 0) { path = u.Substring(0, qmark); query = u.Substring(qmark + 1); }

            var kv = HttpUtility.ParseQueryString(query.Replace(';','&'));
            string method = kv["method"] ?? kv["m"];
            
            // If no method parameter found, use the path as the method name
            if (string.IsNullOrWhiteSpace(method) && !string.IsNullOrWhiteSpace(path))
            {
                method = path;
                // Remove trailing slash if present
                if (method.EndsWith("/"))
                {
                    method = method.Substring(0, method.Length - 1);
                }
            }
                        
            if (string.IsNullOrWhiteSpace(method))
            {
                var url = kv["url"];
                if (!string.IsNullOrWhiteSpace(url)) { form.Navigate(url); form.BringToFront(); return; }
                var script = kv["script"];
                if (!string.IsNullOrWhiteSpace(script)) { RunScriptByName(script, kv["args"] ?? ""); return; }
            }
            switch ((method ?? "").ToLowerInvariant())
            {
                case "openentity":
                case "oe":
                    form.OpenEntity(kv["entityType"] ?? "", kv["entityId"] ?? "", kv["emailAddress"] ?? "", kv["phoneNumber"] ?? "", kv["address"] ?? "", kv["name"] ?? "", kv["ContactName"] ?? "" );
                    form.BringToFront();
                    return;
                case "navigate":
                case "nav":
                    form.Navigate(kv["url"] ?? "");
                    form.BringToFront();
                    break;
                case "exec":
                    form.ExecuteScriptAsync(kv["js"] ?? "");
                    break;
                case "execres":
                    form.ExecuteScriptWithResultBlocking(kv["js"] ?? "");
                    break;
                case "call":
                    {
                        string name = kv["name"] ?? kv["n"] ?? "";
                        string argsJson = kv["args"] ?? "[]";
                        string js = $@"(async()=>{{try{{const fn=(window['{name}']||{name});const args={argsJson};const val=await fn.apply(window,args);return {{ok:true,result:val}};}}catch(e){{return {{ok:false,error:String(e)}}}}}})()";
                        form.ExecuteScriptWithResultBlocking(js);
                        break;
                    }
                case "script":
                    RunScriptByName(kv["name"] ?? kv["script"] ?? "", kv["args"] ?? "");
                    break;
                case "setHomePage":
                    {
                        string url = kv["url"] ?? "";
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            Program.Config.StartupUrl = url;
                            Program.Config.Save();
                        }
                        break;
                    }
                default:
                    if (path.Equals("call", System.StringComparison.OrdinalIgnoreCase))
                    {
                        string name = kv["name"] ?? "";
                        string argsJson = kv["args"] ?? "[]";
                        string js = $@"(async()=>{{try{{const fn=(window['{name}']||{name});const args={argsJson};const val=await fn.apply(window,args);return {{ok:true,result:val}};}}catch(e){{return {{ok:false,error:String(e)}}}}}})()";
                        form.ExecuteScriptWithResultBlocking(js);
                    }
                    break;
            }
        }

        private static bool RunScriptByName(string name, string args)
        {
            var s = Program.Config.JsScripts.Find(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (s == null) return false;
            return RunScriptFile(s.File, args);
        }

        private static bool RunScriptFile(string filePath, string args)
        {
            try
            {
                string cscript = Environment.ExpandEnvironmentVariables(@"%SystemRoot%\System32\cscript.exe");
                if (!File.Exists(cscript)) return false;
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = cscript,
                    Arguments = $"//E:JScript //nologo \"{filePath}\" {args ?? ""}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var p = System.Diagnostics.Process.Start(psi);
                return p != null;
            }
            catch { return false; }
        }
    }
}
