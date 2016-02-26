using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace img_hash2
{
    class Program
    {
        static void Main(string[] args)
        {
            var output_folder = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "downloaded");
            var BASE_URL = "http://www.credit-card-logos.com/";
            var client = new System.Net.Http.HttpClient();
            var html = client.GetAsync(BASE_URL).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//img"))
                {
                    string src = link.GetAttributeValue("src", string.Empty);
                    if (src.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
                        continue;
                    var img_url = BASE_URL + src;
                    var img_bytes = client.GetAsync(img_url).GetAwaiter().GetResult().Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                    // save locally
                    var file_name = img_url.Substring(img_url.LastIndexOf("/") + 1);
                    var full_name = Path.Combine(output_folder, file_name);
                    if (File.Exists(full_name))
                    {
                        file_name = Path.GetFileNameWithoutExtension(file_name) + "_" + System.Environment.TickCount.ToString() + Path.GetExtension(file_name);
                    }
                    Console.WriteLine($"Writing file {img_url} {file_name}");
                    Directory.CreateDirectory(output_folder);
                    File.WriteAllBytes(Path.Combine(output_folder, file_name), img_bytes);
                    string hash = Convert.ToBase64String(sha1.ComputeHash(img_bytes));
                }
            }
        }
    }
}
