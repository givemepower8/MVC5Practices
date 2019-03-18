using System.Collections.Generic;
using Newtonsoft.Json;

namespace MVC5Practices.Utils.DataTables
{
    public class DataTablesRequest
    {
        [JsonProperty(PropertyName = "columns")]
        public List<DataTablesRequestColumn> Columns { get; set; }

        [JsonProperty(PropertyName = "draw")]
        public int Draw { get; set; }

        [JsonProperty(PropertyName = "length")]
        public int Length { get; set; }

        [JsonProperty(PropertyName = "order")]
        public List<DataTablesRequestOrder> Orders { get; set; }

        [JsonProperty(PropertyName = "search")]
        public DataTablesRequestSearch Search { get; set; }

        [JsonProperty(PropertyName = "start")]
        public int Start { get; set; }
    }
}