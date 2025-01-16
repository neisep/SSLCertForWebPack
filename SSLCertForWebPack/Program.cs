using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace SSLCertForWebPack
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            Console.WriteLine($"Working directory: {basePath}");
            Console.WriteLine("Trying to locate SSL certificate used by webpack-dev-server");

            var directories = Directory.GetDirectories(basePath, "webpack-dev-server", SearchOption.AllDirectories);

            if(!directories.Any(x => x.EndsWith("webpack-dev-server")))
            {
                Console.WriteLine("Could not find webpack-dev-server directory. Exiting...");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                return;
            }

            var files = Directory.GetFiles(directories[0], "*.pem", SearchOption.AllDirectories);

            if(!files.Any())
            {
                Console.WriteLine("Could not find any .pem files. Exiting...");
                Console.WriteLine("Press any key to exit...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine($"Found the following SSL certificates: {files[0]}");
            Console.WriteLine("Generate certificate for windows from .pem certificate to .cer");

            string pemContent = File.ReadAllText(files[0]);
            string certificate = ExtractCertificate(pemContent);

            if (!string.IsNullOrEmpty(certificate))
            {
                File.WriteAllText("localhost.cer", certificate);
                AddCertificateToStore("localhost.cer");
            }
            else
            {
                Console.WriteLine("No certificate found in the file.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static string ExtractCertificate(string pemContent)
        {
            var match = Regex.Match(pemContent, @"-----BEGIN CERTIFICATE-----(.*?)-----END CERTIFICATE-----", RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Value;
            }
            return null;
        }

        static void AddCertificateToStore(string certPath)
        {
            try
            {
                X509Certificate2 certificate = new X509Certificate2(certPath);
                using (X509Store store = new X509Store(StoreName.Root, StoreLocation.LocalMachine))
                {
                    store.Open(OpenFlags.ReadWrite);
                    store.Add(certificate);
                    store.Close();
                }
                Console.WriteLine("Certificate added to Trusted Root Certification Authorities.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add certificate to store: {ex.Message}");
            }
        }
    }
}
