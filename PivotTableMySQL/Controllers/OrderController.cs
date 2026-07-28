using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor;

namespace PivotTableMySQL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly string ConnectionString;

        public OrderController(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("MySQL") ?? throw new InvalidOperationException("The MySQL connection string is not configured.");
        }

        [HttpPost]
        public object Post([FromBody] DataManagerRequest request)
        {
            _ = request;

            List<Order> dataSource = GetOrderData();

            return new
            {
                result = dataSource,
                count = dataSource.Count
            };
        }

        private List<Order> GetOrderData()
        {
            const string query =
                @"SELECT orderid,
                         customername,
                         employeeid,
                         shipcity,
                         freight
                  FROM orders
                  ORDER BY orderid";

            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            using MySqlCommand command = new(query, connection);
            using MySqlDataAdapter adapter = new(command);

            DataTable dataTable = new();
            adapter.Fill(dataTable);

            return (from DataRow row in dataTable.Rows
                    select new Order
                    {
                        OrderID = Convert.ToInt32(row["orderid"]),
                        CustomerName = row["customername"].ToString(),
                        EmployeeID = Convert.ToInt32(row["employeeid"]),
                        ShipCity = row.IsNull("shipcity")
                            ? null
                            : row["shipcity"].ToString(),
                        Freight = row.IsNull("freight")
                            ? null
                            : Convert.ToDecimal(row["freight"])
                    }).ToList();
        }

        [HttpPost("Insert")]
        public IActionResult Insert([FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order
                || string.IsNullOrWhiteSpace(order.CustomerName)
                || !order.EmployeeID.HasValue)
            {
                return BadRequest(
                    "CustomerName and EmployeeID are required.");
            }

            const string query =
                @"INSERT INTO orders
                    (customername, freight, shipcity, employeeid)
                  VALUES
                    (@customername, @freight, @shipcity, @employeeid);";

            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@customername",
                order.CustomerName);

            command.Parameters.AddWithValue("@freight",
                order.Freight.HasValue
                    ? order.Freight.Value
                    : DBNull.Value);

            command.Parameters.AddWithValue("@shipcity",
                order.ShipCity ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@employeeid",
                order.EmployeeID.Value);

            command.ExecuteNonQuery();

            order.OrderID = Convert.ToInt32(command.LastInsertedId);

            return Ok(order);
        }

        [HttpPost("Update")]
        public IActionResult Update([FromBody] CRUDModel<Order> value)
        {
            if (value.Value is not Order order
                || !order.OrderID.HasValue
                || string.IsNullOrWhiteSpace(order.CustomerName)
                || !order.EmployeeID.HasValue)
            {
                return BadRequest(
                    "OrderID, CustomerName and EmployeeID are required.");
            }

            const string query =
                @"UPDATE orders
                  SET customername = @customername,
                      freight      = @freight,
                      employeeid   = @employeeid,
                      shipcity     = @shipcity
                  WHERE orderid    = @orderid";

            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@customername",
                order.CustomerName);

            command.Parameters.AddWithValue("@freight",
                order.Freight.HasValue
                    ? order.Freight.Value
                    : DBNull.Value);

            command.Parameters.AddWithValue("@employeeid",
                order.EmployeeID.Value);

            command.Parameters.AddWithValue("@shipcity",
                order.ShipCity ?? (object)DBNull.Value);

            command.Parameters.AddWithValue("@orderid",
                order.OrderID.Value);

            return command.ExecuteNonQuery() == 0
                ? NotFound()
                : Ok(order);
        }

        [HttpPost("Delete")]
        public IActionResult Delete([FromBody] CRUDModel<Order> value)
        {
            if (!int.TryParse(value.Key?.ToString(), out int orderId))
            {
                return BadRequest("A numeric order key is required.");
            }

            const string query =
                @"DELETE FROM orders
                  WHERE orderid = @orderid";

            using MySqlConnection connection = new(ConnectionString);
            connection.Open();

            using MySqlCommand command = new(query, connection);

            command.Parameters.AddWithValue("@orderid", orderId);

            return command.ExecuteNonQuery() == 0
                ? NotFound()
                : NoContent();
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
            [JsonPropertyName("action")]
            public string? Action { get; set; }

            [JsonPropertyName("keyColumn")]
            public string? KeyColumn { get; set; }

            [JsonPropertyName("key")]
            public object? Key { get; set; }

            [JsonPropertyName("value")]
            public T? Value { get; set; }

            [JsonPropertyName("added")]
            public List<T>? Added { get; set; }

            [JsonPropertyName("changed")]
            public List<T>? Changed { get; set; }

            [JsonPropertyName("deleted")]
            public List<T>? Deleted { get; set; }

            [JsonPropertyName("params")]
            public IDictionary<string, object>? Params { get; set; }
        }
    }
}