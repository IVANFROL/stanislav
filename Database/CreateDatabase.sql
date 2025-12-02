-- База данных для магазина автозапчастей
-- Создание базы данных

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'AutoPartsStore')
BEGIN
    CREATE DATABASE AutoPartsStore;
END
GO

USE AutoPartsStore;
GO

-- Таблица категорий запчастей
CREATE TABLE Categories (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(500)
);
GO

-- Таблица поставщиков
CREATE TABLE Suppliers (
    SupplierID INT PRIMARY KEY IDENTITY(1,1),
    CompanyName NVARCHAR(200) NOT NULL,
    ContactPerson NVARCHAR(100),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(300),
    City NVARCHAR(100),
    Country NVARCHAR(100),
    CreatedDate DATETIME DEFAULT GETDATE()
);
GO

-- Таблица пользователей системы
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL DEFAULT 'User',
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastLoginDate DATETIME,
    CHECK (Role IN ('Admin', 'User'))
);
GO

-- Таблица клиентов
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY(1,1),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(300),
    City NVARCHAR(100),
    RegistrationDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);
GO

-- Таблица товаров (автозапчастей)
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductCode NVARCHAR(50) NOT NULL UNIQUE,
    ProductName NVARCHAR(200) NOT NULL,
    CategoryID INT NOT NULL,
    SupplierID INT,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL,
    CostPrice DECIMAL(18,2),
    StockQuantity INT DEFAULT 0,
    MinStockLevel INT DEFAULT 5,
    Unit NVARCHAR(20) DEFAULT 'шт',
    CreatedDate DATETIME DEFAULT GETDATE(),
    LastModifiedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
);
GO

-- Таблица заказов
CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    CustomerID INT NOT NULL,
    OrderDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(50) DEFAULT 'Новый',
    TotalAmount DECIMAL(18,2) DEFAULT 0,
    Notes NVARCHAR(1000),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);
GO

-- Таблица позиций заказа
CREATE TABLE OrderItems (
    OrderItemID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- Таблица продаж
CREATE TABLE Sales (
    SaleID INT PRIMARY KEY IDENTITY(1,1),
    SaleNumber NVARCHAR(50) NOT NULL UNIQUE,
    OrderID INT,
    CustomerID INT,
    SaleDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    PaymentMethod NVARCHAR(50),
    Notes NVARCHAR(1000),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID)
);
GO

-- Таблица позиций продажи
CREATE TABLE SaleItems (
    SaleItemID INT PRIMARY KEY IDENTITY(1,1),
    SaleID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (SaleID) REFERENCES Sales(SaleID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- Таблица поставок
CREATE TABLE Deliveries (
    DeliveryID INT PRIMARY KEY IDENTITY(1,1),
    DeliveryNumber NVARCHAR(50) NOT NULL UNIQUE,
    SupplierID INT NOT NULL,
    DeliveryDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) DEFAULT 0,
    Status NVARCHAR(50) DEFAULT 'Ожидается',
    Notes NVARCHAR(1000),
    FOREIGN KEY (SupplierID) REFERENCES Suppliers(SupplierID)
);
GO

-- Таблица позиций поставки
CREATE TABLE DeliveryItems (
    DeliveryItemID INT PRIMARY KEY IDENTITY(1,1),
    DeliveryID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (DeliveryID) REFERENCES Deliveries(DeliveryID) ON DELETE CASCADE,
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
GO

-- Индексы для оптимизации запросов
CREATE INDEX IX_Products_CategoryID ON Products(CategoryID);
CREATE INDEX IX_Products_SupplierID ON Products(SupplierID);
CREATE INDEX IX_Products_ProductCode ON Products(ProductCode);
CREATE INDEX IX_Orders_CustomerID ON Orders(CustomerID);
CREATE INDEX IX_Orders_OrderDate ON Orders(OrderDate);
CREATE INDEX IX_OrderItems_OrderID ON OrderItems(OrderID);
CREATE INDEX IX_OrderItems_ProductID ON OrderItems(ProductID);
CREATE INDEX IX_Sales_CustomerID ON Sales(CustomerID);
CREATE INDEX IX_Sales_SaleDate ON Sales(SaleDate);
CREATE INDEX IX_SaleItems_SaleID ON SaleItems(SaleID);
CREATE INDEX IX_Deliveries_SupplierID ON Deliveries(SupplierID);
GO

-- Триггер для обновления количества товара при продаже
CREATE TRIGGER trg_UpdateStockOnSale
ON SaleItems
AFTER INSERT
AS
BEGIN
    UPDATE Products
    SET StockQuantity = StockQuantity - i.Quantity,
        LastModifiedDate = GETDATE()
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;
END;
GO

-- Триггер для обновления количества товара при поставке
CREATE TRIGGER trg_UpdateStockOnDelivery
ON DeliveryItems
AFTER INSERT
AS
BEGIN
    UPDATE Products
    SET StockQuantity = StockQuantity + i.Quantity,
        LastModifiedDate = GETDATE()
    FROM Products p
    INNER JOIN inserted i ON p.ProductID = i.ProductID;
END;
GO

-- Триггер для расчета суммы заказа
CREATE TRIGGER trg_UpdateOrderTotal
ON OrderItems
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    DECLARE @OrderID INT;
    
    IF EXISTS (SELECT * FROM inserted)
        SET @OrderID = (SELECT OrderID FROM inserted);
    ELSE IF EXISTS (SELECT * FROM deleted)
        SET @OrderID = (SELECT OrderID FROM deleted);
    
    UPDATE Orders
    SET TotalAmount = (
        SELECT ISNULL(SUM(Subtotal), 0)
        FROM OrderItems
        WHERE OrderID = @OrderID
    )
    WHERE OrderID = @OrderID;
END;
GO

-- Заполнение начальными данными
INSERT INTO Categories (CategoryName, Description) VALUES
('Двигатель', 'Запчасти для двигателя'),
('Трансмиссия', 'Запчасти для трансмиссии'),
('Подвеска', 'Элементы подвески'),
('Тормозная система', 'Тормозные колодки, диски, суппорты'),
('Электрика', 'Электронные компоненты и проводка'),
('Кузов', 'Кузовные элементы'),
('Салон', 'Элементы салона');

INSERT INTO Suppliers (CompanyName, ContactPerson, Phone, Email, Address, City, Country) VALUES
('ООО "АвтоДеталь"', 'Иванов Иван', '+7 (495) 123-45-67', 'info@avtodetal.ru', 'ул. Автозаводская, 1', 'Москва', 'Россия'),
('ЗАО "Запчасти Плюс"', 'Петров Петр', '+7 (812) 234-56-78', 'sales@zapchasti-plus.ru', 'пр. Невский, 100', 'Санкт-Петербург', 'Россия'),
('ИП Смирнов', 'Смирнов Сергей', '+7 (495) 345-67-89', 'smirnov@mail.ru', 'ул. Ленина, 50', 'Москва', 'Россия');

-- Создание пользователей системы
-- Пароль для админа: admin123 (хэш SHA256)
-- Пароль для пользователя: user123 (хэш SHA256)
INSERT INTO Users (Username, PasswordHash, FullName, Role, IsActive) VALUES
('admin', '240BE518FABD2724DDB6F04EEB1DA5967448D7E831C08C8FA822809F74C720A9', 'Администратор системы', 'Admin', 1),
('user', 'E606E38B0D8C19B24CF0EE3808183162EA7CD63FF7912DBB22B5E803286B4446', 'Обычный пользователь', 'User', 1);

GO

PRINT 'База данных AutoPartsStore успешно создана!';
