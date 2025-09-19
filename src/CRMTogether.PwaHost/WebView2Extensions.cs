using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CRMTogether.PwaHost
{
    public static class WebView2Extensions
    {
        public static async Task<string> ExecuteScriptFunctionAsync(this WebView2Wrapper webView, string functionName, params object[] parameters)
        {
            string script = functionName + "(";
            for (int i = 0; i < parameters.Length; i++)
            {
                script += JsonConvert.SerializeObject(parameters[i]);
                if (i < parameters.Length - 1)
                {
                    script += ", ";
                }
            }
            script += ");";
            return await webView.ExecuteScriptAsync(script);
        }

        public static string GetVersion(this WebView2Wrapper webView)
        {
            if (webView.CoreWebView2 != null)
            {
                return webView.CoreWebView2.Environment.BrowserVersionString;
            }

            return string.Empty;
        }

        /// <summary>
        /// to remove double quotes from string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string RemoveQuotes(this String data)
        {
            if (!string.IsNullOrEmpty(data))
            {
                data = data.Replace("\"", "");
            }

            return data;
        }
    }
}
