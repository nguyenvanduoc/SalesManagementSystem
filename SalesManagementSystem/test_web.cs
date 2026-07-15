using System;
using System.Net;

class Program
{
    static void Main()
    {
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        try {
            using (WebClient wc = new WebClient()) {
                string html = wc.DownloadString("https://localhost:44326/KHO_HaoHut/_Detail?id=8");
                Console.WriteLine("SUCCESS");
            }
        } catch (WebException ex) {
            Console.WriteLine(ex.Message);
            if (ex.Response != null) {
                using (var reader = new System.IO.StreamReader(ex.Response.GetResponseStream())) {
                    string error = reader.ReadToEnd();
                    int start = error.IndexOf("<title>");
                    int end = error.IndexOf("</title>");
                    if (start > 0 && end > start) {
                        Console.WriteLine("ERROR TITLE: " + error.Substring(start + 7, end - start - 7));
                    }
                    else {
                        Console.WriteLine(error.Substring(0, Math.Min(500, error.Length)));
                    }
                }
            }
        }
    }
}
