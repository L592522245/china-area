using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace ChinaArea
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var home = Get("http://xzqh.mca.gov.cn/map");
            var regexJson = new Regex("var json = ([^;]+);");
            if (regexJson.IsMatch(home))
            {
                var list = new List<DataItem>();
                var encode = Encoding.GetEncoding(936);
                var json = regexJson.Match(home).Groups[1].Value;
                var categoryItems = JsonConvert.DeserializeObject<List<DataItem>>(json);
                foreach (var shengji in categoryItems)
                {
                    Console.WriteLine(shengji);
                    list.Add(shengji);
                    var key = HttpUtility.UrlEncode(shengji.ShengJi, encode);
                    var page = Get($"http://202.108.98.30/defaultQuery?shengji={key}&diji=-1&xianji=-1");
                    var doc = new HtmlDocument();
                    doc.LoadHtml(page);
                    var trList = doc.DocumentNode.SelectNodes("//tr");
                    if (trList != null)
                    {
                        DataItem diji = null;
                        foreach (var tr in trList)
                        {
                            var tdList = tr.SelectNodes("./td");
                            if (tdList != null && tdList.Count == 7)
                            {
                                DataItem current;
                                if (
                                    diji == null
                                    || !string.IsNullOrWhiteSpace(tr.GetAttributeValue("flag", string.Empty))
                                )
                                {
                                    diji = JsonConvert
                                        .DeserializeObject<DataItem>(JsonConvert.SerializeObject(shengji));
                                    current = diji;
                                    current.DiJi = tdList[0].InnerText.Trim('+', '☆', ' ', '\r', '\n');
                                }
                                else
                                {
                                    current = JsonConvert
                                        .DeserializeObject<DataItem>(JsonConvert.SerializeObject(diji));
                                    current.XianJi = tdList[0].InnerText;
                                }
                                current.ZhuDi = tdList[1].InnerText.Trim();
                                current.RenKou = tdList[2].InnerText.Trim();
                                current.MianJi = tdList[3].InnerText.Trim();
                                current.QuHuaDaiMa = tdList[4].InnerText.Trim();
                                current.QuHao = tdList[5].InnerText.Trim();
                                current.YouBian = tdList[6].InnerText.Trim();
                                if (!string.IsNullOrWhiteSpace(current.QuHuaDaiMa))
                                {
                                    list.Add(current);
                                }
                            }
                        }
                    }
                }

                var dir = AppDomain.CurrentDomain.BaseDirectory;
                for (int i = 0; i < 4; i++)
                {
                    dir = Path.GetDirectoryName(dir);
                }
                dir = Path.Combine(dir, "data");
                var dt = DateTime.Now.ToString("yyyyMMdd");
                var dateDir = Path.Combine(dir, dt);
                if (!Directory.Exists(dateDir))
                {
                    Directory.CreateDirectory(dateDir);
                }
                using (var fs = File.OpenWrite(Path.Combine(dateDir, "China.csv")))
                {
                    fs.SetLength(0);
                    using (var sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine("行政区划代码,省级,地级,县级,区号,邮编");
                        foreach (var item in list)
                        {
                            sw.WriteLine(string.Join(",",
                                item.QuHuaDaiMa,
                                item.ShengJiName,
                                item.DiJi,
                                item.XianJi,
                                item.QuHao,
                                item.YouBian
                            ));
                        }
                    }
                }
                File.WriteAllText(Path.Combine(dateDir, "China.json"), JsonConvert.SerializeObject(list, Formatting.Indented), Encoding.UTF8);

                File.Copy(Path.Combine(dateDir, "China.csv"), Path.Combine(dir, "China.csv"), true);
                File.Copy(Path.Combine(dateDir, "China.json"), Path.Combine(dir, "China.json"), true);
                Console.WriteLine("Done.");
            }
            Console.ReadLine();
        }

        public static string Get(string url)
        {
            using (var web = new WebClient())
            {
                var bytes = web.DownloadData(url);
                var encode = Encoding.GetEncoding(936);
                return encode.GetString(bytes);
            }
        }
    }
}