using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DataEncryption.Providers;
using System;
using System.Linq;

namespace AesSample;

static class Program
{
    static void Main()
    {
        using SqliteConnection connection = new("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseSqlite(connection)
            .Options;

        // AES key randomly generated at each run.
        CryptoAesKeyInfo keyInfo = AesCryptoProvider.GenerateKey(CryptoAesKeySize.AES256Bits);
        byte[] encryptionKey = keyInfo.Key;
        byte[] encryptionIV = keyInfo.IV;
        var encryptionProvider = new AesCryptoProvider(encryptionKey, encryptionIV);

        using (var context = new DatabaseContext(options, encryptionProvider))
        {
            context.Database.EnsureCreated();

            var user = new UserEntity
            {
                FirstName = "Royce",
                LastName = "Lark",
                Email = "Royce@RoyceLark.com",
                Notes = "Hello world!",
                EncryptedData = new byte[2] { 1, 2 },
                EncryptedDataAsString = new byte[2] { 3, 4 }
            };

            context.Users.Add(user);
            context.SaveChanges();

            Console.WriteLine($"Users count: {context.Users.Count()}");
        }

        using (var context = new DatabaseContext(options, encryptionProvider))
        {
            UserEntity user = context.Users.First();

            Console.WriteLine($"User: {user.FirstName} {user.LastName} - {user.Email} (Notes: {user.Notes})");
        }
    }
}
