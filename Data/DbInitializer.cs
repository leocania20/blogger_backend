using blogger_backend.Models;
using Microsoft.AspNetCore.Identity;

namespace blogger_backend.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context, IPasswordHasher<UserModel> hasher)
        {
        }
    }
}