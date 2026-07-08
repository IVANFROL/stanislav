-- Заполнение базы данных тестовыми данными (ИСПРАВЛЕННАЯ ВЕРСИЯ)
-- Используйте этот файл, если InsertSampleData.sql не работает

USE AutoPartsStore;
GO

-- Установка формата даты
SET DATEFORMAT ymd;
GO

-- Добавление клиентов
INSERT INTO Customers (FirstName, LastName, Phone, Email, Address, City) VALUES
('Александр', 'Кузнецов', '+7 (495) 111-11-11', 'kuznetsov@mail.ru', 'ул. Пушкина, 10', 'Москва'),
('Мария', 'Соколова', '+7 (495) 222-22-22', 'sokolova@mail.ru', 'ул. Гагарина, 20', 'Москва'),
('Дмитрий', 'Волков', '+7 (812) 333-33-33', 'volkov@mail.ru', 'пр. Мира, 30', 'Санкт-Петербург'),
('Елена', 'Новикова', '+7 (495) 444-44-44', 'novikova@mail.ru', 'ул. Мира, 40', 'Москва'),
('Сергей', 'Федоров', '+7 (495) 555-55-55', 'fedorov@mail.ru', 'ул. Ленина, 50', 'Москва');
GO

-- Добавление товаров
INSERT INTO Products (ProductCode, ProductName, CategoryID, SupplierID, Description, Price, CostPrice, StockQuantity, MinStockLevel) VALUES
('ENG001', 'Масляный фильтр', 1, 1, 'Масляный фильтр для двигателя', 850.00, 500.00, 50, 10),
('ENG002', 'Воздушный фильтр', 1, 1, 'Воздушный фильтр стандартный', 650.00, 400.00, 30, 10),
('ENG003', 'Свечи зажигания (комплект)', 1, 1, 'Комплект свечей зажигания (4 шт)', 1200.00, 800.00, 25, 5),
('TRANS001', 'Масло трансмиссионное 1л', 2, 2, 'Трансмиссионное масло 75W-90', 950.00, 600.00, 40, 10),
('TRANS002', 'Фильтр АКПП', 2, 2, 'Фильтр для автоматической коробки передач', 2500.00, 1500.00, 15, 5),
('SUSP001', 'Амортизатор передний', 3, 3, 'Передний амортизатор', 3500.00, 2200.00, 20, 5),
('SUSP002', 'Стойка стабилизатора', 3, 3, 'Стойка стабилизатора поперечной устойчивости', 1200.00, 750.00, 35, 10),
('BRAKE001', 'Тормозные колодки передние', 4, 1, 'Комплект передних тормозных колодок', 2800.00, 1800.00, 30, 10),
('BRAKE002', 'Тормозной диск передний', 4, 1, 'Тормозной диск передний', 3200.00, 2000.00, 20, 5),
('ELEC001', 'Аккумулятор 60Ач', 5, 2, 'Аккумуляторная батарея 60 Ач', 5500.00, 3500.00, 12, 3),
('ELEC002', 'Генератор', 5, 2, 'Генератор 14V 90A', 8500.00, 5500.00, 8, 2),
('BODY001', 'Фара передняя левая', 6, 3, 'Передняя фара левая', 4500.00, 2800.00, 10, 3),
('BODY002', 'Бампер передний', 6, 3, 'Передний бампер', 12000.00, 7500.00, 5, 2),
('INT001', 'Коврики в салон (комплект)', 7, 1, 'Комплект ковриков в салон', 2500.00, 1500.00, 25, 10),
('INT002', 'Чехлы на сиденья', 7, 1, 'Чехлы на передние сиденья', 3500.00, 2200.00, 15, 5);
GO

-- Добавление заказов (используем GETDATE() с вычитанием дней для надежности)
INSERT INTO Orders (OrderNumber, CustomerID, OrderDate, Status, Notes) VALUES
('ORD-2024-001', 1, DATEADD(day, -350, GETDATE()), 'Выполнен', 'Срочный заказ'),
('ORD-2024-002', 2, DATEADD(day, -349, GETDATE()), 'В обработке', NULL),
('ORD-2024-003', 3, DATEADD(day, -348, GETDATE()), 'Новый', NULL);
GO

-- Проверка, что заказы созданы
IF NOT EXISTS (SELECT 1 FROM Orders WHERE OrderID = 1)
BEGIN
    PRINT 'ОШИБКА: Заказ с ID=1 не создан!';
END
GO

-- Добавление позиций заказов
INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice, Subtotal) VALUES
(1, 1, 2, 850.00, 1700.00),
(1, 2, 1, 650.00, 650.00),
(1, 3, 1, 1200.00, 1200.00),
(2, 8, 1, 2800.00, 2800.00),
(2, 9, 2, 3200.00, 6400.00),
(3, 10, 1, 5500.00, 5500.00);
GO

-- Добавление продаж
INSERT INTO Sales (SaleNumber, OrderID, CustomerID, SaleDate, TotalAmount, PaymentMethod, Notes) VALUES
('SALE-2024-001', 1, 1, DATEADD(day, -350, GETDATE()), 3550.00, 'Наличные', 'Оплачено полностью');
GO

-- Проверка, что продажа создана
IF NOT EXISTS (SELECT 1 FROM Sales WHERE SaleID = 1)
BEGIN
    PRINT 'ОШИБКА: Продажа с ID=1 не создана!';
END
GO

-- Добавление позиций продаж
INSERT INTO SaleItems (SaleID, ProductID, Quantity, UnitPrice, Subtotal) VALUES
(1, 1, 2, 850.00, 1700.00),
(1, 2, 1, 650.00, 650.00),
(1, 3, 1, 1200.00, 1200.00);
GO

-- Добавление поставок
INSERT INTO Deliveries (DeliveryNumber, SupplierID, DeliveryDate, TotalAmount, Status, Notes) VALUES
('DEL-2024-001', 1, DATEADD(day, -355, GETDATE()), 50000.00, 'Получено', 'Первая поставка'),
('DEL-2024-002', 2, DATEADD(day, -353, GETDATE()), 35000.00, 'Получено', NULL);
GO

-- Добавление позиций поставок
INSERT INTO DeliveryItems (DeliveryID, ProductID, Quantity, UnitPrice, Subtotal) VALUES
(1, 1, 50, 500.00, 25000.00),
(1, 2, 30, 400.00, 12000.00),
(1, 8, 30, 1800.00, 54000.00),
(2, 4, 40, 600.00, 24000.00),
(2, 5, 15, 1500.00, 22500.00);
GO

PRINT 'Тестовые данные успешно добавлены!';
PRINT 'Проверка данных:';

DECLARE @CustomersCount INT;
DECLARE @ProductsCount INT;
DECLARE @OrdersCount INT;
DECLARE @SalesCount INT;

SELECT @CustomersCount = COUNT(*) FROM Customers;
SELECT @ProductsCount = COUNT(*) FROM Products;
SELECT @OrdersCount = COUNT(*) FROM Orders;
SELECT @SalesCount = COUNT(*) FROM Sales;

PRINT 'Клиентов: ' + CAST(@CustomersCount AS VARCHAR);
PRINT 'Товаров: ' + CAST(@ProductsCount AS VARCHAR);
PRINT 'Заказов: ' + CAST(@OrdersCount AS VARCHAR);
PRINT 'Продаж: ' + CAST(@SalesCount AS VARCHAR);
GO

