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

        public bool AddCategory(Category category)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    INSERT INTO Categories (CategoryName, Description)
                    VALUES (@CategoryName, @Description)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    command.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateCategory(Category category)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE Categories 
                    SET CategoryName = @CategoryName, Description = @Description
                    WHERE CategoryID = @CategoryID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryID", category.CategoryID);
                    command.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                    command.Parameters.AddWithValue("@Description", (object?)category.Description ?? DBNull.Value);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteCategory(int categoryID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Categories WHERE CategoryID = @CategoryID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CategoryID", categoryID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
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

        public bool AddSupplier(Supplier supplier)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    INSERT INTO Suppliers (CompanyName, ContactPerson, Phone, Email, Address, City, Country)
                    VALUES (@CompanyName, @ContactPerson, @Phone, @Email, @Address, @City, @Country)";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CompanyName", supplier.CompanyName);
                    command.Parameters.AddWithValue("@ContactPerson", (object?)supplier.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object?)supplier.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object?)supplier.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object?)supplier.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object?)supplier.City ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Country", (object?)supplier.Country ?? DBNull.Value);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool UpdateSupplier(Supplier supplier)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    UPDATE Suppliers 
                    SET CompanyName = @CompanyName, ContactPerson = @ContactPerson,
                        Phone = @Phone, Email = @Email, 
                        Address = @Address, City = @City, Country = @Country
                    WHERE SupplierID = @SupplierID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplier.SupplierID);
                    command.Parameters.AddWithValue("@CompanyName", supplier.CompanyName);
                    command.Parameters.AddWithValue("@ContactPerson", (object?)supplier.ContactPerson ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Phone", (object?)supplier.Phone ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Email", (object?)supplier.Email ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object?)supplier.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@City", (object?)supplier.City ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Country", (object?)supplier.Country ?? DBNull.Value);
                    
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public Supplier? GetSupplier(int supplierID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT SupplierID, CompanyName, ContactPerson, Phone, Email, Address, City, Country, CreatedDate
                    FROM Suppliers
                    WHERE SupplierID = @SupplierID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierID);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Supplier
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
                            };
                        }
                    }
                }
            }
            return null;
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

        // Методы для отчетов
        public List<SalesReportItem> GetSalesReport(DateTime fromDate, DateTime toDate)
        {
            var report = new List<SalesReportItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT s.SaleID, s.SaleNumber, s.SaleDate,
                           ISNULL(c.FirstName + ' ' + c.LastName, 'Без клиента') as CustomerName,
                           s.TotalAmount, ISNULL(s.PaymentMethod, '') as PaymentMethod,
                           (SELECT COUNT(*) FROM SaleItems si WHERE si.SaleID = s.SaleID) as ProductCount
                    FROM Sales s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID
                    WHERE s.SaleDate >= @FromDate AND s.SaleDate <= @ToDate
                    ORDER BY s.SaleDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    command.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.Add(new SalesReportItem
                            {
                                SaleID = reader.GetInt32("SaleID"),
                                SaleNumber = reader.GetString("SaleNumber"),
                                SaleDate = reader.GetDateTime("SaleDate"),
                                CustomerName = reader.GetString("CustomerName"),
                                TotalAmount = reader.GetDecimal("TotalAmount"),
                                PaymentMethod = reader.GetString("PaymentMethod"),
                                ProductCount = reader.GetInt32("ProductCount")
                            });
                        }
                    }
                }
            }
            return report;
        }

        public List<PurchasesReportItem> GetPurchasesReport(DateTime fromDate, DateTime toDate)
        {
            var report = new List<PurchasesReportItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT d.DeliveryID, d.DeliveryNumber, d.DeliveryDate,
                           s.CompanyName as SupplierName,
                           d.TotalAmount, ISNULL(d.Status, '') as Status,
                           (SELECT COUNT(*) FROM DeliveryItems di WHERE di.DeliveryID = d.DeliveryID) as ProductCount
                    FROM Deliveries d
                    INNER JOIN Suppliers s ON d.SupplierID = s.SupplierID
                    WHERE d.DeliveryDate >= @FromDate AND d.DeliveryDate <= @ToDate
                    ORDER BY d.DeliveryDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FromDate", fromDate.Date);
                    command.Parameters.AddWithValue("@ToDate", toDate.Date.AddDays(1).AddSeconds(-1));
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            report.Add(new PurchasesReportItem
                            {
                                DeliveryID = reader.GetInt32("DeliveryID"),
                                DeliveryNumber = reader.GetString("DeliveryNumber"),
                                DeliveryDate = reader.GetDateTime("DeliveryDate"),
                                SupplierName = reader.GetString("SupplierName"),
                                TotalAmount = reader.GetDecimal("TotalAmount"),
                                Status = reader.GetString("Status"),
                                ProductCount = reader.GetInt32("ProductCount")
                            });
                        }
                    }
                }
            }
            return report;
        }

        // Методы для работы с поставками
        public List<Delivery> GetDeliveries()
        {
            var deliveries = new List<Delivery>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT d.DeliveryID, d.DeliveryNumber, d.SupplierID,
                           s.CompanyName as SupplierName,
                           d.DeliveryDate, d.TotalAmount, d.Status, d.Notes
                    FROM Deliveries d
                    LEFT JOIN Suppliers s ON d.SupplierID = s.SupplierID
                    ORDER BY d.DeliveryDate DESC";
                
                using (var command = new SqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        deliveries.Add(new Delivery
                        {
                            DeliveryID = reader.GetInt32("DeliveryID"),
                            DeliveryNumber = reader.GetString("DeliveryNumber"),
                            SupplierID = reader.GetInt32("SupplierID"),
                            SupplierName = reader.IsDBNull("SupplierName") ? "" : reader.GetString("SupplierName"),
                            DeliveryDate = reader.GetDateTime("DeliveryDate"),
                            TotalAmount = reader.GetDecimal("TotalAmount"),
                            Status = reader.IsDBNull("Status") ? "" : reader.GetString("Status"),
                            Notes = reader.IsDBNull("Notes") ? "" : reader.GetString("Notes")
                        });
                    }
                }
            }
            return deliveries;
        }

        public List<DeliveryItem> GetDeliveryItems(int deliveryID)
        {
            var items = new List<DeliveryItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT di.DeliveryItemID, di.DeliveryID, di.ProductID,
                           p.ProductName, p.ProductCode,
                           di.Quantity, di.UnitPrice, di.Subtotal
                    FROM DeliveryItems di
                    INNER JOIN Products p ON di.ProductID = p.ProductID
                    WHERE di.DeliveryID = @DeliveryID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DeliveryID", deliveryID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new DeliveryItem
                            {
                                DeliveryItemID = reader.GetInt32("DeliveryItemID"),
                                DeliveryID = reader.GetInt32("DeliveryID"),
                                ProductID = reader.GetInt32("ProductID"),
                                ProductName = reader.GetString("ProductName"),
                                ProductCode = reader.GetString("ProductCode"),
                                Quantity = reader.GetInt32("Quantity"),
                                UnitPrice = reader.GetDecimal("UnitPrice"),
                                Subtotal = reader.GetDecimal("Subtotal")
                            });
                        }
                    }
                }
            }
            return items;
        }

        // Методы для получения деталей заказов
        public List<OrderItem> GetOrderItems(int orderID)
        {
            var items = new List<OrderItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT oi.OrderItemID, oi.OrderID, oi.ProductID,
                           p.ProductName, p.ProductCode,
                           oi.Quantity, oi.UnitPrice, oi.Subtotal
                    FROM OrderItems oi
                    INNER JOIN Products p ON oi.ProductID = p.ProductID
                    WHERE oi.OrderID = @OrderID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new OrderItem
                            {
                                OrderItemID = reader.GetInt32("OrderItemID"),
                                OrderID = reader.GetInt32("OrderID"),
                                ProductID = reader.GetInt32("ProductID"),
                                ProductName = reader.GetString("ProductName"),
                                ProductCode = reader.GetString("ProductCode"),
                                Quantity = reader.GetInt32("Quantity"),
                                UnitPrice = reader.GetDecimal("UnitPrice"),
                                Subtotal = reader.GetDecimal("Subtotal")
                            });
                        }
                    }
                }
            }
            return items;
        }

        // Методы для получения деталей продаж
        public List<SaleItem> GetSaleItems(int saleID)
        {
            var items = new List<SaleItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = @"
                    SELECT si.SaleItemID, si.SaleID, si.ProductID,
                           p.ProductName, p.ProductCode,
                           si.Quantity, si.UnitPrice, si.Subtotal
                    FROM SaleItems si
                    INNER JOIN Products p ON si.ProductID = p.ProductID
                    WHERE si.SaleID = @SaleID";
                
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SaleID", saleID);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new SaleItem
                            {
                                SaleItemID = reader.GetInt32("SaleItemID"),
                                SaleID = reader.GetInt32("SaleID"),
                                ProductID = reader.GetInt32("ProductID"),
                                ProductName = reader.GetString("ProductName"),
                                ProductCode = reader.GetString("ProductCode"),
                                Quantity = reader.GetInt32("Quantity"),
                                UnitPrice = reader.GetDecimal("UnitPrice"),
                                Subtotal = reader.GetDecimal("Subtotal")
                            });
                        }
                    }
                }
            }
            return items;
        }

        // Метод для получения статистики
        public Statistics GetStatistics()
        {
            var stats = new Statistics();
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                // Общее количество товаров
                var query1 = "SELECT COUNT(*) FROM Products";
                using (var cmd = new SqlCommand(query1, connection))
                {
                    stats.TotalProducts = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Товары с низким остатком
                var query2 = "SELECT COUNT(*) FROM Products WHERE StockQuantity <= MinStockLevel";
                using (var cmd = new SqlCommand(query2, connection))
                {
                    stats.LowStockProducts = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Общая стоимость товаров на складе
                var query3 = "SELECT ISNULL(SUM(StockQuantity * CostPrice), 0) FROM Products";
                using (var cmd = new SqlCommand(query3, connection))
                {
                    stats.TotalStockValue = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                // Количество клиентов
                var query4 = "SELECT COUNT(*) FROM Customers WHERE IsActive = 1";
                using (var cmd = new SqlCommand(query4, connection))
                {
                    stats.TotalCustomers = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Продажи за месяц
                var query5 = @"
                    SELECT ISNULL(SUM(TotalAmount), 0), COUNT(*)
                    FROM Sales
                    WHERE SaleDate >= DATEADD(month, -1, GETDATE())";
                using (var cmd = new SqlCommand(query5, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.MonthlySalesAmount = reader.GetDecimal(0);
                        stats.MonthlySalesCount = reader.GetInt32(1);
                    }
                }

                // Заказы за месяц
                var query6 = @"
                    SELECT ISNULL(SUM(TotalAmount), 0), COUNT(*)
                    FROM Orders
                    WHERE OrderDate >= DATEADD(month, -1, GETDATE())";
                using (var cmd = new SqlCommand(query6, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.MonthlyOrdersAmount = reader.GetDecimal(0);
                        stats.MonthlyOrdersCount = reader.GetInt32(1);
                    }
                }

                // Продажи за сегодня
                var query7 = @"
                    SELECT ISNULL(SUM(TotalAmount), 0), COUNT(*)
                    FROM Sales
                    WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)";
                using (var cmd = new SqlCommand(query7, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        stats.TodaySalesAmount = reader.GetDecimal(0);
                        stats.TodaySalesCount = reader.GetInt32(1);
                    }
                }

                // Общая сумма всех продаж
                var query8 = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Sales";
                using (var cmd = new SqlCommand(query8, connection))
                {
                    stats.TotalSalesAmount = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                // Общая сумма всех заказов
                var query9 = "SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders";
                using (var cmd = new SqlCommand(query9, connection))
                {
                    stats.TotalOrdersAmount = Convert.ToDecimal(cmd.ExecuteScalar());
                }

                // Количество поставщиков
                var query10 = "SELECT COUNT(*) FROM Suppliers";
                using (var cmd = new SqlCommand(query10, connection))
                {
                    stats.TotalSuppliers = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Количество категорий
                var query11 = "SELECT COUNT(*) FROM Categories";
                using (var cmd = new SqlCommand(query11, connection))
                {
                    stats.TotalCategories = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // Средний чек
                var query12 = @"
                    SELECT ISNULL(AVG(TotalAmount), 0)
                    FROM Sales
                    WHERE SaleDate >= DATEADD(month, -1, GETDATE())";
                using (var cmd = new SqlCommand(query12, connection))
                {
                    stats.AverageCheck = Convert.ToDecimal(cmd.ExecuteScalar());
                }
            }
            return stats;
        }

        // Методы удаления
        public bool DeleteOrder(int orderID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Orders WHERE OrderID = @OrderID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@OrderID", orderID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteSale(int saleID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Sales WHERE SaleID = @SaleID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SaleID", saleID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteCustomer(int customerID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "UPDATE Customers SET IsActive = 0 WHERE CustomerID = @CustomerID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteSupplier(int supplierID)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var query = "DELETE FROM Suppliers WHERE SupplierID = @SupplierID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SupplierID", supplierID);
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }
    }

    // Классы для отчетов
    public class SalesReportItem
    {
        public int SaleID { get; set; }
        public string SaleNumber { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class PurchasesReportItem
    {
        public int DeliveryID { get; set; }
        public string DeliveryNumber { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class Delivery
    {
        public int DeliveryID { get; set; }
        public string DeliveryNumber { get; set; } = string.Empty;
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime DeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public List<DeliveryItem> Items { get; set; } = new List<DeliveryItem>();
    }

    public class DeliveryItem
    {
        public int DeliveryItemID { get; set; }
        public int DeliveryID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class Statistics
    {
        public int TotalProducts { get; set; }
        public int LowStockProducts { get; set; }
        public decimal TotalStockValue { get; set; }
        public int TotalCustomers { get; set; }
        public decimal MonthlySalesAmount { get; set; }
        public int MonthlySalesCount { get; set; }
        public decimal MonthlyOrdersAmount { get; set; }
        public int MonthlyOrdersCount { get; set; }
        public decimal TodaySalesAmount { get; set; }
        public int TodaySalesCount { get; set; }
        public decimal TotalSalesAmount { get; set; }
        public decimal TotalOrdersAmount { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalCategories { get; set; }
        public decimal AverageCheck { get; set; }
    }
}
