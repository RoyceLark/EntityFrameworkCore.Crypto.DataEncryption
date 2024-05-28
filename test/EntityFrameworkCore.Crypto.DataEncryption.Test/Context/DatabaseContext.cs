namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;

public class DatabaseContext : DbContext
{
    private readonly IEncryptionCryptoProvider _encryptionProvider;

    public DbSet<AuthorEntity> Authors { get; set; }

    public DbSet<BookEntity> Books { get; set; }

    public DatabaseContext(DbContextOptions options)
        : base(options)
    { }

#pragma warning disable S3427 // Method overloads with default parameter values should not overlap
    public DatabaseContext(DbContextOptions options, IEncryptionCryptoProvider encryptionProvider = null)
#pragma warning restore S3427 // Method overloads with default parameter values should not overlap
        : base(options)
    {
        _encryptionProvider = encryptionProvider ?? throw new System.ArgumentNullException(nameof(encryptionProvider));
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseEncryption(_encryptionProvider);
    }
}
