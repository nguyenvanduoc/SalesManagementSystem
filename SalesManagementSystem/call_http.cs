using System;
using System.Net;

class Program {
    static void Main() {
        ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
        try {
            var client = new WebClient();
            string response = client.DownloadString("https://localhost:44326/DeploySP.aspx");
            Console.WriteLine(response);
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
