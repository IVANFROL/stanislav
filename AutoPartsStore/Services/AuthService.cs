using System.Security.Cryptography;
using System.Text;
using AutoPartsStore.Database;
using AutoPartsStore.Models;

namespace AutoPartsStore.Services
{
    public class AuthService
    {
        private readonly DatabaseHelper _dbHelper;
        private User? _currentUser;

        public AuthService(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public User? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public bool IsAdmin => _currentUser?.IsAdmin ?? false;

        public bool Login(string username, string password)
        {
            var user = _dbHelper.GetUserByUsername(username);
            if (user == null || !user.IsActive)
                return false;

            var passwordHash = HashPassword(password);
            if (user.PasswordHash != passwordHash)
                return false;

            _currentUser = user;
            _dbHelper.UpdateLastLoginDate(user.UserID);
            return true;
        }

        public void Logout()
        {
            _currentUser = null;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToUpper();
            }
        }

        public bool HasPermission(string permission)
        {
            if (!IsAuthenticated)
                return false;

            // Админ имеет все права
            if (IsAdmin)
                return true;

            // Обычный пользователь может только просматривать и создавать заказы/продажи
            return permission switch
            {
                "ViewProducts" => true,
                "ViewOrders" => true,
                "ViewSales" => true,
                "ViewCustomers" => true,
                "CreateOrder" => true,
                "CreateSale" => true,
                "EditProduct" => false,
                "DeleteProduct" => false,
                "EditCustomer" => false,
                "DeleteCustomer" => false,
                "EditSupplier" => false,
                "DeleteSupplier" => false,
                "ManageUsers" => false,
                _ => false
            };
        }
    }
}



