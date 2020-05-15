using System;
using System.IO;

namespace SplashScraper.Controller {

    public static class ControllerHelper {

        public static void WriteLine(string txt, bool error = false) {
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");
            var path = "Logs/Log.txt";

            if (error) {
                path = "Logs/Errors.txt";
                txt = "[" + System.DateTime.Now + "] " + txt;
            }
            
            var info = new FileInfo(path);

            if (info.Exists && info.Length >= 26214400) {
                File.Move(info.FullName, "Logs/" + Path.GetFileNameWithoutExtension(info.FullName) + "_" + DateTime.Now.ToString("yyyyMMdd_HH_mm_ss") + info.Extension);
            }

            using (var file = File.AppendText(path)) {
                file.WriteLine(txt);
            }

            Console.WriteLine(txt);
        }

    }

}