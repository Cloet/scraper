using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SplashScraper.Controller {

    public static class SplashController {

        public static async Task<Stream> ScrapeWebPage(string endpoint, string url, string luaScript) {
            var client = new HttpClient();

            var requestUrl = $"{endpoint}/execute?url={url}&wait=10&timeout=90.0";
            requestUrl = $"{requestUrl}&lua_source={luaScript}";
            
            // Do request
            var response = await client.GetAsync(requestUrl);

            return await response.Content.ReadAsStreamAsync();
        }

        public static string StreamToString(Stream stream) {
            var reader = new StreamReader(stream);
            var txt = reader.ReadToEnd();
            reader.Dispose();
            return txt;
        }

        public static async Task<string> ScrapeWebPageToString(string endpoint, string url, string luaScript) {
            return StreamToString(await ScrapeWebPage(endpoint,url,luaScript));
        }

    }


}