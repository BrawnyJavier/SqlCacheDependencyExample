using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Caching;
using System.Web.Http;

namespace SQLCacheDependencyExample.Controllers
{
    public class DefaultController : ApiController
    {
        /// <summary>
        /// Just a dummy DTO class of northwind's products.
        /// </summary>
        public class Product
        {
            public int ProductID { get; set; }
            public string ProductName { get; set; }
            public Decimal UnitPrice { get; set; }
            public string QuantityPerUnit { get; set; }

        }

        [HttpGet, Route("GetProducts")]
        public object GetProducts()
        {

            // If the cache is not been added, then we have to query the database for the data.
            if (HttpRuntime.Cache["Products"] == null)
            {
                // Gets the connection string from the web config.
                var northwindDatabaseConnectionString = ConfigurationManager.ConnectionStrings["northwindDatabase"].ConnectionString;

                SqlConnection Connection = new SqlConnection(northwindDatabaseConnectionString);

                Connection.Open();

                SqlCommand cmd = new SqlCommand("SELECT TOP (10) * FROM [dbo].[Products]", Connection);
                SqlDataReader reader = cmd.ExecuteReader();
                List<Product> R = new List<Product>();
                while (reader.Read())
                {
                    var value = new Product
                    {
                        ProductName = (string)reader["ProductName"],
                        ProductID = (int)reader["ProductID"],
                        QuantityPerUnit = (string)reader["QuantityPerUnit"],
                        UnitPrice = (Decimal)reader["UnitPrice"]
                    };
                    R.Add(value);
                }

                // Create the Sql Cache dependency object to use it on ur cache.
                // First parameter is the database name, second the table to create the dependency.
                SqlCacheDependency SQL_DEPENDENCY = new SqlCacheDependency("NORTHWND", "Products");


                // Lets insert the list of products into the cache,
                // so for further requests, the data is already available.
                HttpRuntime.Cache.Insert("Products", R, SQL_DEPENDENCY);

                // Lets send the data!
                return new { FROM = "Database", VALUES = R };
            }

            // The cache has been created, lets send it rather than query the database!
            else
            {
                return new { FROM = "CACHE", VALUES = HttpRuntime.Cache["Products"] as List<Product> };

            }

        }

    }
}
