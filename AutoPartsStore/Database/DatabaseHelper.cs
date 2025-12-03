using System.Data;
using System.Data.SqlClient;
using AutoPartsStore.Models;

namespace AutoPartsStore.Database
{
    public class DatabaseHelper
    {
        private readonly string _connectionString;

        public DatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Методы для работы с товарами
        public List<Product> GetProducts()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT p.ProductID, p.ProductCode, p.ProductName, p.CategoryID, 
                           c.CategoryName, p.SupplierID, s.CompanyName as SupplierName,
                           p.Description, p.Price, p.CostPrice, p.StockQuantity, 
                           p.MinStockLevel, p.Unit, p.CreatedDate, p.LastModifiedDate
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                    LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                    ORDER BY p.ProductName";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductID = reader.GetInt32("ProductID"),
                            ProductCode = reader.GetString("ProductCode"),
                            ProductName = reader.GetString("ProductName"),
                            CategoryID = reader.GetInt32("CategoryID"),
                            CategoryName = reader.IsDBNull("CategoryName") ? "" : reader.GetString("CategoryName"),
                            SupplierID = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                            SupplierName = reader.IsDBNull("SupplierName") ? "" : reader.GetString("SupplierName"),
                            Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                            Price = reader.GetDecimal("Price"),
                            CostPrice = reader.IsDBNull("CostPrice") ? 0 : reader.GetDecimal("CostPrice"),
                            StockQuantity = reader.GetInt32("StockQuantity"),
                            MinStockLevel = reader.GetInt32("MinStockLevel"),
                            Unit = reader.GetString("Unit"),
                            CreatedDate = reader.GetDateTime("CreatedDate"),
                            LastModifiedDate = reader.GetDateTime("LastModifiedDate")
                        });
                    }
                }
            }
            return products;
        }

        public Product? GetProduct(int productID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT p.ProductID, p.ProductCode, p.ProductName, p.CategoryID, 
                           c.CategoryName, p.SupplierID, s.CompanyName as SupplierName,
                           p.Description, p.Price, p.CostPrice, p.StockQuantity, 
                           p.MinStockLevel, p.Unit, p.CreatedDate, p.LastModifiedDate
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                    LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                    WHERE p.ProductID = @ProductID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Product
                            {
                                ProductID = reader.GetInt32("ProductID"),
                                ProductCode = reader.GetString("ProductCode"),
                                ProductName = reader.GetString("ProductName"),
                                CategoryID = reader.GetInt32("CategoryID"),
                                CategoryName = reader.IsDBNull("CategoryName") ? "" : reader.GetString("CategoryName"),
                                SupplierID = reader.IsDBNull("SupplierID") ? null : reader.GetInt32("SupplierID"),
                                SupplierName = reader.IsDBNull("SupplierName") ? "" : reader.GetString("SupplierName"),
                                Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description"),
                                Price = reader.GetDecimal("Price"),
                                CostPrice = reader.IsDBNull("CostPrice") ? 0 : reader.GetDecimal("CostPrice"),
                                StockQuantity = reader.GetInt32("StockQuantity"),
                                MinStockLevel = reader.GetInt32("MinStockLevel"),
                                Unit = reader.GetString("Unit"),
                                CreatedDate = reader.GetDateTime("CreatedDate"),
                                LastModifiedDate = reader.GetDateTime("LastModifiedDate")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public bool AddProduct(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    INSERT INTO Products (ProductCode, ProductName, CategoryID, SupplierID, 
                                        Description, Price, CostPrice, StockQuantity, MinStockLevel, Unit)
                    VALUES (@ProductCode, @ProductName, @CategoryID, @SupplierID, 
                           @Description, @Price, @CostPrice, @StockQuantity, @MinStockLevel, @Unit)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    command.Parameters.AddWithValue("@SupplierID", product.SupplierID.HasValue ? (object)product.SupplierID.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@CostPrice", product.CostPrice);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                    command.Parameters.AddWithValue("@Unit", product.Unit);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateProduct(Product product)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE Products 
                    SET ProductCode = @ProductCode, ProductName = @ProductName, 
                        CategoryID = @CategoryID, SupplierID = @SupplierID,
                        Description = @Description, Price = @Price, CostPrice = @CostPrice,
                        StockQuantity = @StockQuantity, MinStockLevel = @MinStockLevel, 
                        Unit = @Unit, LastModifiedDate = GETDATE()
                    WHERE ProductID = @ProductID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", product.ProductID);
                    command.Parameters.AddWithValue("@ProductCode", product.ProductCode);
                    command.Parameters.AddWithValue("@ProductName", product.ProductName);
                    command.Parameters.AddWithValue("@CategoryID", product.CategoryID);
                    command.Parameters.AddWithValue("@SupplierID", product.SupplierID.HasValue ? (object)product.SupplierID.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object?)product.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Price", product.Price);
                    command.Parameters.AddWithValue("@CostPrice", product.CostPrice);
                    command.Parameters.AddWithValue("@StockQuantity", product.StockQuantity);
                    command.Parameters.AddWithValue("@MinStockLevel", product.MinStockLevel);
                    command.Parameters.AddWithValue("@Unit", product.Unit);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteProduct(int productID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Products WHERE ProductID = @ProductID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        // Методы для работы с категориями
        public List<Category> GetCategories()
        {
            var categories = new List<Category>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT CategoryID, CategoryName, Description FROM Categories ORDER BY CategoryName";
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category
                        {
                            CategoryID = reader.GetInt32("CategoryID"),
                            CategoryName = reader.GetString("CategoryName"),
                            Description = reader.IsDBNull("Description") ? "" : reader.GetString("Description")
                        });
                    }
                }
            }
            return categories;
        }

        // Методы для работы с клиентами
        public List<Customer> GetCustomers()
        {
            var customers = new List<Customer>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT CustomerID, FirstName, LastName, Phone, Email, Address, City, 
                           RegistrationDate, IsActive
                    FROM Customers
                    WHERE IsActive = 1
                    ORDER BY LastName, FirstName";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            CustomerID = reader.GetInt32("CustomerID"),
                            FirstName = reader.GetString("FirstName"),
                            LastName = reader.GetString("LastName"),
                            Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                            Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                            Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                            City = reader.IsDBNull("City") ? "" : reader.GetString("City"),
                            RegistrationDate = reader.GetDateTime("RegistrationDate"),
                            IsActive = reader.GetBoolean("IsActive")
                        });
                    }
                }
            }
            return customers;
        }

        public bool AddCustomer(Customer customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    INSERT INTO Customers (FirstName, LastName, Phone, Email, Address, City)
                    VALUES (@FirstName, @LastName, @Phone, @Email, @Address, @City)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    command.Parameters.AddWithValue("@LastName", customer.LastName);
                    command.Parameters.AddWithValue("@Phone", (object?)customer.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object?)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object?)customer.City ?? DBNull.Value);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateCustomer(Customer customer)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE Customers 
                    SET FirstName = @FirstName, LastName = @LastName, 
                        Phone = @Phone, Email = @Email, 
                        Address = @Address, City = @City
                    WHERE CustomerID = @CustomerID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customer.CustomerID);
                    command.Parameters.AddWithValue("@FirstName", customer.FirstName);
                    command.Parameters.AddWithValue("@LastName", customer.LastName);
                    command.Parameters.AddWithValue("@Phone", (object?)customer.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object?)customer.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object?)customer.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object?)customer.City ?? DBNull.Value);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public Customer? GetCustomer(int customerID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT CustomerID, FirstName, LastName, Phone, Email, Address, City, 
                           RegistrationDate, IsActive
                    FROM Customers
                    WHERE CustomerID = @CustomerID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Customer
                            {
                                CustomerID = reader.GetInt32("CustomerID"),
                                FirstName = reader.GetString("FirstName"),
                                LastName = reader.GetString("LastName"),
                                Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                                Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                                Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                                City = reader.IsDBNull("City") ? "" : reader.GetString("City"),
                                RegistrationDate = reader.GetDateTime("RegistrationDate"),
                                IsActive = reader.GetBoolean("IsActive")
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Методы для работы с поставщиками
        public List<Supplier> GetSuppliers()
        {
            var suppliers = new List<Supplier>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "SELECT SupplierID, CompanyName, ContactPerson, Phone, Email, Address, City, Country, CreatedDate FROM Suppliers ORDER BY CompanyName";
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        suppliers.Add(new Supplier
                        {
                            SupplierID = reader.GetInt32("SupplierID"),
                            CompanyName = reader.GetString("CompanyName"),
                            ContactPerson = reader.IsDBNull("ContactPerson") ? "" : reader.GetString("ContactPerson"),
                            Phone = reader.IsDBNull("Phone") ? "" : reader.GetString("Phone"),
                            Email = reader.IsDBNull("Email") ? "" : reader.GetString("Email"),
                            Address = reader.IsDBNull("Address") ? "" : reader.GetString("Address"),
                            City = reader.IsDBNull("City") ? "" : reader.GetString("City"),
                            Country = reader.IsDBNull("Country") ? "" : reader.GetString("Country"),
                            CreatedDate = reader.GetDateTime("CreatedDate")
                        });
                    }
                }
            }
            return suppliers;
        }

        // Методы для работы с заказами
        public List<Order> GetOrders()
        {
            var orders = new List<Order>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT o.OrderID, o.OrderNumber, o.CustomerID, 
                           c.FirstName + ' ' + c.LastName as CustomerName,
                           o.OrderDate, o.Status, o.TotalAmount, o.Notes
                    FROM Orders o
                    INNER JOIN Customers c ON o.CustomerID = c.CustomerID
                    ORDER BY o.OrderDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orders.Add(new Order
                        {
                            OrderID = reader.GetInt32("OrderID"),
                            OrderNumber = reader.GetString("OrderNumber"),
                            CustomerID = reader.GetInt32("CustomerID"),
                            CustomerName = reader.GetString("CustomerName"),
                            OrderDate = reader.GetDateTime("OrderDate"),
                            Status = reader.GetString("Status"),
                            TotalAmount = reader.GetDecimal("TotalAmount"),
                            Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes")
                        });
                    }
                }
            }
            return orders;
        }

        public int CreateOrder(Order order)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Генерация номера заказа
                        var orderNumber = $"ORD-{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                        
                        // Создание заказа
                        var orderQuery = @"
                            INSERT INTO Orders (OrderNumber, CustomerID, OrderDate, Status, Notes)
                            VALUES (@OrderNumber, @CustomerID, @OrderDate, @Status, @Notes);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
                        
                        int orderID;
                        using (var command = new SqlCommand(orderQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                            command.Parameters.AddWithValue("@CustomerID", order.CustomerID);
                            command.Parameters.AddWithValue("@OrderDate", order.OrderDate);
                            command.Parameters.AddWithValue("@Status", order.Status);
                            command.Parameters.AddWithValue("@Notes", (object?)order.Notes ?? DBNull.Value);
                            
                            orderID = (int)command.ExecuteScalar();
                        }
                        
                        // Добавление позиций заказа
                        foreach (var item in order.Items)
                        {
                            var itemQuery = @"
                                INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice, Subtotal)
                                VALUES (@OrderID, @ProductID, @Quantity, @UnitPrice, @Subtotal)";
                            
                            using (var command = new SqlCommand(itemQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@OrderID", orderID);
                                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                command.Parameters.AddWithValue("@Subtotal", item.Subtotal);
                                
                                command.ExecuteNonQuery();
                            }
                        }
                        
                        transaction.Commit();
                        return orderID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Методы для работы с продажами
        public List<Sale> GetSales()
        {
            var sales = new List<Sale>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT s.SaleID, s.SaleNumber, s.OrderID, s.CustomerID,
                           c.FirstName + ' ' + c.LastName as CustomerName,
                           s.SaleDate, s.TotalAmount, s.PaymentMethod, s.Notes
                    FROM Sales s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    ORDER BY s.SaleDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new Sale
                        {
                            SaleID = reader.GetInt32("SaleID"),
                            SaleNumber = reader.GetString("SaleNumber"),
                            OrderID = reader.IsDBNull("OrderID") ? null : reader.GetInt32("OrderID"),
                            CustomerID = reader.IsDBNull("CustomerID") ? null : reader.GetInt32("CustomerID"),
                            CustomerName = reader.IsDBNull("CustomerName") ? "" : reader.GetString("CustomerName"),
                            SaleDate = reader.GetDateTime("SaleDate"),
                            TotalAmount = reader.GetDecimal("TotalAmount"),
                            PaymentMethod = reader.IsDBNull("PaymentMethod") ? "" : reader.GetString("PaymentMethod"),
                            Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes")
                        });
                    }
                }
            }
            return sales;
        }

        public int CreateSale(Sale sale)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Генерация номера продажи
                        var saleNumber = $"SALE-{DateTime.Now:yyyy-MM-dd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
                        
                        // Создание продажи
                        var saleQuery = @"
                            INSERT INTO Sales (SaleNumber, OrderID, CustomerID, SaleDate, TotalAmount, PaymentMethod, Notes)
                            VALUES (@SaleNumber, @OrderID, @CustomerID, @SaleDate, @TotalAmount, @PaymentMethod, @Notes);
                            SELECT CAST(SCOPE_IDENTITY() as int)";
                        
                        int saleID;
                        using (var command = new SqlCommand(saleQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@SaleNumber", saleNumber);
                            command.Parameters.AddWithValue("@OrderID", sale.OrderID.HasValue ? (object)sale.OrderID.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@CustomerID", sale.CustomerID.HasValue ? (object)sale.CustomerID.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@SaleDate", sale.SaleDate);
                            command.Parameters.AddWithValue("@TotalAmount", sale.TotalAmount);
                            command.Parameters.AddWithValue("@PaymentMethod", (object?)sale.PaymentMethod ?? DBNull.Value);
                            command.Parameters.AddWithValue("@Notes", (object?)sale.Notes ?? DBNull.Value);
                            
                            saleID = (int)command.ExecuteScalar();
                        }
                        
                        // Добавление позиций продажи
                        foreach (var item in sale.Items)
                        {
                            var itemQuery = @"
                                INSERT INTO SaleItems (SaleID, ProductID, Quantity, UnitPrice, Subtotal)
                                VALUES (@SaleID, @ProductID, @Quantity, @UnitPrice, @Subtotal)";
                            
                            using (var command = new SqlCommand(itemQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@SaleID", saleID);
                                command.Parameters.AddWithValue("@ProductID", item.ProductID);
                                command.Parameters.AddWithValue("@Quantity", item.Quantity);
                                command.Parameters.AddWithValue("@UnitPrice", item.UnitPrice);
                                command.Parameters.AddWithValue("@Subtotal", item.Subtotal);
                                
                                command.ExecuteNonQuery();
                            }
                        }
                        
                        transaction.Commit();
                        return saleID;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        // Получение товаров с низким остатком
        public List<Product> GetLowStockProducts()
        {
            var products = new List<Product>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT p.ProductID, p.ProductCode, p.ProductName, p.CategoryID, 
                           c.CategoryName, p.StockQuantity, p.MinStockLevel
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID
                    WHERE p.StockQuantity <= p.MinStockLevel
                    ORDER BY p.StockQuantity";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        products.Add(new Product
                        {
                            ProductID = reader.GetInt32("ProductID"),
                            ProductCode = reader.GetString("ProductCode"),
                            ProductName = reader.GetString("ProductName"),
                            CategoryID = reader.GetInt32("CategoryID"),
                            CategoryName = reader.IsDBNull("CategoryName") ? "" : reader.GetString("CategoryName"),
                            StockQuantity = reader.GetInt32("StockQuantity"),
                            MinStockLevel = reader.GetInt32("MinStockLevel")
                        });
                    }
                }
            }
            return products;
        }

        // Методы для работы с пользователями
        public User? GetUserByUsername(string username)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT UserID, Username, PasswordHash, FullName, Role, IsActive, CreatedDate, LastLoginDate
                    FROM Users
                    WHERE Username = @Username AND IsActive = 1";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                UserID = reader.GetInt32("UserID"),
                                Username = reader.GetString("Username"),
                                PasswordHash = reader.GetString("PasswordHash"),
                                FullName = reader.GetString("FullName"),
                                Role = reader.GetString("Role"),
                                IsActive = reader.GetBoolean("IsActive"),
                                CreatedDate = reader.GetDateTime("CreatedDate"),
                                LastLoginDate = reader.IsDBNull("LastLoginDate") ? null : reader.GetDateTime("LastLoginDate")
                            };
                        }
                    }
                }
            }
            return null;
        }

        public void UpdateLastLoginDate(int userID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Users SET LastLoginDate = GETDATE() WHERE UserID = @UserID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userID);
                    command.ExecuteNonQuery();
                }
            }
        }

        public List<User> GetUsers()
        {
            var users = new List<User>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT UserID, Username, PasswordHash, FullName, Role, IsActive, CreatedDate, LastLoginDate
                    FROM Users
                    ORDER BY Username";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(new User
                        {
                            UserID = reader.GetInt32("UserID"),
                            Username = reader.GetString("Username"),
                            PasswordHash = reader.GetString("PasswordHash"),
                            FullName = reader.GetString("FullName"),
                            Role = reader.GetString("Role"),
                            IsActive = reader.GetBoolean("IsActive"),
                            CreatedDate = reader.GetDateTime("CreatedDate"),
                            LastLoginDate = reader.IsDBNull("LastLoginDate") ? null : reader.GetDateTime("LastLoginDate")
                        });
                    }
                }
            }
            return users;
        }
    }
}
