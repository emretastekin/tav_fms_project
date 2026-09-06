using System;
using System.Collections.Generic;

namespace FMS.Security.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        
        // Firebase Authentication'dan gelen eşsiz kimlik (UID). 
        // Sisteme giriş yapan kişiyi bu kimlikle kendi veritabanımızda bulacağız.
        public string FirebaseUid { get; set; } 
        public string Email { get; set; }
        public bool IsActive { get; set; } = true;

        // Bire-Çok İlişki Bağlantısı
        public ICollection<UserRole> UserRoles { get; set; }
    }
}