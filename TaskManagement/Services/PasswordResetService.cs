using System;
using System.Security.Cryptography;
using System.Text;
using TaskManagement.Models;
using TaskManagement.DataAccess;

namespace TaskManagement.Services
{
    public class PasswordResetService
    {
        private readonly PasswordResetTokenRepository _tokenRepository;
        public PasswordResetService()
        {
            _tokenRepository = new PasswordResetTokenRepository();
        }
        public string GenerateToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
            }
        }
        public void CreateToken(int userId, string token, DateTime expiration)
        {
            _tokenRepository.DeleteByUserId(userId); // Önceki tokenları sil
            _tokenRepository.Add(new PasswordResetToken { UserId = userId, Token = token, Expiration = expiration });
        }
        public PasswordResetToken GetByToken(string token)
        {
            return _tokenRepository.GetByToken(token);
        }
        public void DeleteByUserId(int userId)
        {
            _tokenRepository.DeleteByUserId(userId);
        }
    }
} 