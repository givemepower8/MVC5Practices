using System.Collections;
using Newtonsoft.Json;

namespace MVC5Practices.Utils.DataTables
{
    public class DataTablesResult
    {
        [JsonProperty(PropertyName = "draw")]
        public int Draw { get; set; }

        [JsonProperty(PropertyName = "recordsTotal")]
        public int RecordsTotal { get; set; }

        [JsonProperty(PropertyName = "recordsFiltered")]
        public int RecordsFiltered { get; set; }

        [JsonProperty(PropertyName = "data")]
        public IEnumerable Data { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}