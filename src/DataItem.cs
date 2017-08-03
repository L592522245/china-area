using System;
using Newtonsoft.Json;

namespace ChinaArea
{
    public class DataItem
    {
        [JsonProperty("quHuaDaiMa")]
        public string QuHuaDaiMa { get; set; }

        [JsonProperty("shengji")]
        public string ShengJi { get; set; }

        public string ShengJiName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ShengJi))
                {
                    var x = ShengJi.IndexOf("(", StringComparison.CurrentCultureIgnoreCase);
                    if (x != -1)
                    {
                        return ShengJi.Substring(0, x);
                    }
                }
                return ShengJi;
            }
        }

        [JsonProperty("diji")]
        public string DiJi { get; set; }

        [JsonProperty("xianji")]
        public string XianJi { get; set; }


        [JsonProperty("quhao")]
        public string QuHao { get; set; }

        public string ZhuDi { get; set; }
        public string RenKou { get; set; }
        public string MianJi { get; set; }
        public string YouBian { get; set; }


        public override string ToString()
        {
            return string.Join(" ", QuHuaDaiMa, ShengJi, DiJi, XianJi);
        }
    }
}