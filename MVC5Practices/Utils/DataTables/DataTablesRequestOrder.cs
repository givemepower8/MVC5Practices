using Newtonsoft.Json;

namespace MVC5Practices.Utils.DataTables
{
    public class DataTablesRequestOrder
    {
        [JsonProperty(PropertyName = "column")]
        public int Column { get; set; }

        [JsonProperty(PropertyName = "dir")]
        public string Direction { get; set; }
    }
}