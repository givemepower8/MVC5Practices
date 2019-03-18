using Newtonsoft.Json;

namespace MVC5Practices.Utils.DataTables
{
    public class DataTablesRequestColumn
    {
        [JsonProperty(PropertyName = "data")]
        public string Data { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "orderable")]
        public bool Orderable { get; set; }

        [JsonProperty(PropertyName = "search")]
        public DataTablesRequestSearch Search { get; set; }

        [JsonProperty(PropertyName = "searchable")]
        public bool Searchable { get; set; }
    }
}