using Microsoft.AspNetCore.Mvc;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor;
using MySql.Data.MySqlClient;
using System.Data;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
// using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace PivotTableMySQL.Controllers
{
    [ApiController]
    public class OrderController : ControllerBase
    {
        string ConnectionString =
            "Server=localhost;Port=3306;Database=Orders;Uid=root;Pwd=password@123;";

        [HttpPost]
        [Route("api/[controller]")]
        public object Post([FromBody] DataManagerRequest DataManagerRequest)
        {
            IEnumerable<Order> DataSource = GetOrderData();
            int count = DataSource.Cast<Order>().Count();

            return new { result = DataSource, count = count };
        }

        [Route("api/[controller]")]
        public List<Order> GetOrderData()
        {
            string QueryStr = "SELECT * FROM orders ORDER BY orderid";

            using MySqlConnection sqlConnection = new(ConnectionString);
            sqlConnection.Open();

            using MySqlCommand SqlCommand = new(QueryStr, sqlConnection);
            using MySqlDataAdapter DataAdapter = new(SqlCommand);

            DataTable DataTable = new();
            DataAdapter.Fill(DataTable);

            var DataSource = (from DataRow Data in DataTable.Rows
                              select new Order()
                              {
                                  OrderID = Convert.ToInt32(Data["orderid"]),
                                  CustomerName = Data["customername"].ToString(),
                                  EmployeeID = Convert.ToInt32(Data["employeeid"]),
                                  ShipCity = Data["shipcity"].ToString(),
                                  Freight = Convert.ToDecimal(Data["freight"])
                              }).ToList();

            return DataSource;
        }

        [HttpPost]
        [Route("api/Order/Insert")]
        public void Insert([FromBody] CRUDModel<Order> Value)
        {
            string Query =
                $"INSERT INTO orders " +
                $"(customername, freight, shipcity, employeeid) " +
                $"VALUES " +
                $"('{Value.Value.CustomerName}', " +
                $"{Value.Value.Freight}, " +
                $"'{Value.Value.ShipCity}', " +
                $"{Value.Value.EmployeeID})";

            using MySqlConnection Connection = new(ConnectionString);
            Connection.Open();

            using MySqlCommand Command = new(Query, Connection);
            Command.ExecuteNonQuery();
        }

        [HttpPost]
        [Route("api/Order/Update")]
        public void Update([FromBody] CRUDModel<Order> Value)
        {
            string Query =
                $"UPDATE orders SET " +
                $"customername='{Value.Value.CustomerName}', " +
                $"freight={Value.Value.Freight}, " +
                $"employeeid={Value.Value.EmployeeID}, " +
                $"shipcity='{Value.Value.ShipCity}' " +
                $"WHERE orderid={Value.Value.OrderID}";

            using MySqlConnection Connection = new(ConnectionString);
            Connection.Open();

            using MySqlCommand Command = new(Query, Connection);
            Command.ExecuteNonQuery();
        }

        [HttpPost]
        [Route("api/Order/Delete")]
        public void Delete([FromBody] CRUDModel<Order> Value)
        {
            string Query =
                $"DELETE FROM orders WHERE orderid={Value.Key}";

            using MySqlConnection Connection = new(ConnectionString);
            Connection.Open();

            using MySqlCommand Command = new(Query, Connection);
            Command.ExecuteNonQuery();
        }

        public class Order
        {
            [Key]
            public int? OrderID { get; set; }
            public string? CustomerName { get; set; }
            public int? EmployeeID { get; set; }
            public decimal? Freight { get; set; }
            public string? ShipCity { get; set; }
        }

        public class CRUDModel<T> where T : class
        {
            [JsonProperty("action")]
            public string? Action { get; set; }

            [JsonProperty("keyColumn")]
            public string? KeyColumn { get; set; }

            [JsonProperty("key")]
            public object? Key { get; set; }

            [JsonProperty("value")]
            public T? Value { get; set; }

            [JsonProperty("added")]
            public List<T>? Added { get; set; }

            [JsonProperty("changed")]
            public List<T>? Changed { get; set; }

            [JsonProperty("deleted")]
            public List<T>? Deleted { get; set; }

            [JsonProperty("params")]
            public IDictionary<string, object>? Params { get; set; }
        }
    }
}