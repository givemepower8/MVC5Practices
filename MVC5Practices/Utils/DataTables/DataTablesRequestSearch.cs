using Newtonsoft.Json;

namespace MVC5Practices.Utils.DataTables
{
    public class DataTablesRequestSearch
    {
        [JsonProperty(PropertyName = "regex")]
        public bool Regex { get; set; }

        [JsonProperty(PropertyName = "value")]
        public string Value { get; set; }
    }
}