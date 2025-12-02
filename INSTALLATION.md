# Инструкция по установке

## Шаг 1: Установка SQL Server

Если у вас еще не установлен SQL Server:
1. Скачайте SQL Server Express (бесплатная версия): https://www.microsoft.com/sql-server/sql-server-downloads
2. Установите SQL Server с настройками по умолчанию
3. Убедитесь, что служба SQL Server запущена

## Шаг 2: Создание базы данных

1. Откройте **SQL Server Management Studio (SSMS)**
   - Если SSMS не установлен, скачайте его: https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms

2. Подключитесь к серверу:
   - **Server name:** `localhost` или `localhost\SQLEXPRESS` (для Express)
   - **Authentication:** Windows Authentication (или SQL Server Authentication, если настроено)

3. Откройте файл `Database/CreateDatabase.sql`

4. Выполните скрипт (F5 или кнопка Execute)

5. (Опционально) Выполните `Database/InsertSampleData.sql` для добавления тестовых данных

## Шаг 3: Настройка строки подключения

Откройте файл `AutoPartsStore/Program.cs` и найдите строку:

```csharp
var connectionString = "Server=localhost;Database=AutoPartsStore;Integrated Security=True;TrustServerCertificate=True;";
```

### Для SQL Server Express:
```csharp
var connectionString = "Server=localhost\\SQLEXPRESS;Database=AutoPartsStore;Integrated Security=True;TrustServerCertificate=True;";
```

### Для SQL Server Authentication:
```csharp
var connectionString = "Server=localhost;Database=AutoPartsStore;User Id=sa;Password=ваш_пароль;TrustServerCertificate=True;";
```

## Шаг 4: Открытие проекта в Visual Studio

1. Откройте Visual Studio 2022 или выше
2. Выберите **File → Open → Project/Solution**
3. Перейдите в папку проекта и откройте `AutoPartsStore/AutoPartsStore.csproj`

## Шаг 5: Восстановление пакетов NuGet

1. В Visual Studio откройте **Tools → NuGet Package Manager → Package Manager Console**
2. Выполните команду:
   ```
   dotnet restore
   ```
   Или Visual Studio автоматически восстановит пакеты при открытии проекта

## Шаг 6: Сборка проекта

1. Нажмите **Build → Build Solution** (или Ctrl+Shift+B)
2. Убедитесь, что сборка прошла успешно

## Шаг 7: Запуск приложения

1. Нажмите **F5** или **Debug → Start Debugging**
2. Откроется форма входа

## Учетные записи для входа

### Администратор
- **Логин:** `admin`
- **Пароль:** `admin123`

### Обычный пользователь
- **Логин:** `user`
- **Пароль:** `user123`

## Устранение проблем

### Ошибка подключения к базе данных

**Проблема:** "Cannot open database 'AutoPartsStore' requested by the login"

**Решение:**
1. Убедитесь, что база данных создана (выполните `CreateDatabase.sql`)
2. Проверьте строку подключения в `Program.cs`
3. Убедитесь, что служба SQL Server запущена:
   - Откройте **Services** (services.msc)
   - Найдите **SQL Server (MSSQLSERVER)** или **SQL Server (SQLEXPRESS)**
   - Убедитесь, что статус "Running"

### Ошибка "TrustServerCertificate"

Если используете старую версию SQL Server, уберите `TrustServerCertificate=True` из строки подключения.

### Ошибка при сборке проекта

**Проблема:** Отсутствуют пакеты NuGet

**Решение:**
1. Откройте **Tools → NuGet Package Manager → Manage NuGet Packages for Solution**
2. Нажмите **Restore** для восстановления пакетов
3. Или выполните в Package Manager Console: `Update-Package -reinstall`

## Проверка работы базы данных

Выполните в SSMS:

```sql
USE AutoPartsStore;
SELECT * FROM Users;
SELECT COUNT(*) as ProductsCount FROM Products;
SELECT COUNT(*) as CustomersCount FROM Customers;
```

Если запросы выполняются успешно, база данных настроена правильно.


