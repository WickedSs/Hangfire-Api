using System;
using System.Reflection;
using Newtonsoft.Json;

namespace hangfire_api.Models {
    public class gitRepo {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string language { get; set; }
        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public DateTime pushed_at { get; set; }

        public int forks { get; set; }
        public int open_issues { get; set; }
        [JsonProperty("stargazzers_count")]
        public int stars { get; set; }
        public string url { get; set; }
        [JsonProperty("size_kb")]

        public int size { get; set; }
        public string git_url { get; set; }
        public int watchers { get; set; } 


        public override bool Equals(object obj) {
            if (obj == null || !this.GetType().Equals(obj.GetType())) {
                return false;
            } else {
                gitRepo g = (gitRepo) obj;
                PropertyInfo[] properties = this.GetType().GetProperties();
                foreach(PropertyInfo property in properties) {
                    System.Console.WriteLine(property.Name + property.GetValue(this).Equals(property.GetType()));
                    if (!property.GetValue(this).Equals(property.GetValue(g))) {
                        return false;
                    }
                }
            }
            return true;
        }





        public override string ToString() {
            string str = "";
            PropertyInfo[] properties = typeof(gitRepo).GetProperties();
            foreach(PropertyInfo property in properties) {
                str += property.Name + " = " + property.GetValue(this, null) + "\n";
            }
            return str;

        }
    }
}