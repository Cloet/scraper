using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SplashScraper.Factory;
using SplashScraper.Repository;

namespace SplashScraper.Controller {

    public class BeursController {

        private string _url;

        public string Url { 
            get => _url;
            private set {
                if (string.IsNullOrEmpty(value))
                    throw new ArgumentNullException();

                _url = value;             
            } 
        }

        public int Port { get; private set; }
        
        public string EndPoint { get => $"{Url}:{Port}"; }

        private string _htmlFolder = "";

        private string _script = "";

        public BeursController(string splashUrl, int splashPort, string htmlFolder) {
            RepoManager.Context = new BeursContext();
            
            Url = splashUrl;
            Port = splashPort;
            if (htmlFolder.Substring(htmlFolder.Length-1,1) != "/")
                htmlFolder += "/";

            _htmlFolder = htmlFolder;

        }

        private string UrlPath(string url) {
            var uri = new Uri(url);
            return uri.AbsolutePath.Replace("//","/").Replace("\\","/").Replace("!","").Replace("?","");
        }

        private string LoadScript() {
            if (string.IsNullOrEmpty(_script))
                _script = File.ReadAllText("/mnt/Data/School/Bachelorproef/SplashScraper/scripts/script.lua");
            return _script;
        }

        private List<string> RetrieveLinksFromFile(string file) {
            List<string> links = File.ReadLines(file).ToList();
            return links;
        }
   

        public void ScrapeBeurs() {
            var baseLinks = RetrieveLinksFromFile("links.txt");
            ControllerHelper.WriteLine("Start scraping from beursduivel.be @ " + DateTime.Now.ToLongDateString());
            
            var tasks = new List<Task>();
            for (var i = 0; i < baseLinks.Count; i++) {
                try {
                    var link = new BeursLink(EndPoint, LoadScript(), baseLinks[i]);
                    tasks.Add(link.StartAsync());
                } catch (Exception ex) {
                    ControllerHelper.WriteLine(ex.ToString(), true);
                }
            }

            Task.WaitAll(tasks.ToArray());
            ControllerHelper.WriteLine("Scraping has finished.");
        }

        // private string UrlHost(string url) {
        //     var uri = new Uri(url);
        //     return SanitizeUrl(uri.Host);
        // }

    }

}