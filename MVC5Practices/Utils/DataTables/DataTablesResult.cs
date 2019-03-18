using System.Collections;
using Newtonsoft.Json;

namespace MVC5Practices.Utils.DataTables
{
    public class DataTablesResult
    {
        [JsonProperty(PropertyName = "draw")]
        public int draw { get; set; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public int recordsTotal { get; set; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public int recordsFiltered { get; set; }

        [JsonProperty(PropertyName = "data")]
        public IEnumerable data { get; set; }

        public string error { get; set; }
    }
}