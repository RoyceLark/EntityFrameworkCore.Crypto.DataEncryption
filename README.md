## Disclaimer

<h4 align="center">:warning: This package/product is **not** affiliated with Microsoft. :warning:</h4><br>

the package has been developed for a project of Royce Lark pvt,ltd which suits our use case. It includes a way to encrypt the column data via context.

Royce Lark **do not** take responsability if you use/deploy this in a production environment and loose your encryption key or corrupt your data.

<h4 align="center">:warning: if you want help/support you must buy a support licence. to buy a licence contact web.html123@gmail.com:warning:</h4><br>

## How to install

Install the package from [NuGet](https://www.nuget.org/) or from the `Package Manager Console` :
```powershell
PM> Install-Package EntityFrameworkCore.Crypto.DataEncryption
```

## Supported types

| Type | Default storage type |
|------|----------------------|
| `string` | Base64 string |
| `byte[]` | BINARY |


## How to use

`EntityFrameworkCore.Crypto.DataEncryption` supports 2 differents initialization methods:
* Attribute
* Fluent configuration

Depending on the initialization method you will use, you will need to decorate your `string` or `byte[]` properties of your entities with the `[CryptoEncrypted]` attribute or use the fluent `IsEncrypted()` method in your model configuration process.
To use an encryption provider on your EF Core model, and enable the encryption on the `CryptoModelBuilder`. 

### Example with `CryptoAesProvider` and attribute

```csharp
public class UserEntity
{
	public int Id { get; set; }
	
	[CryptoEncrypted]
	public string Username { get; set; }
	
	[CryptoEncrypted]
	public string Password { get; set; }
	
	public int Age { get; set; }
}

public class DatabaseContext : DbContext
{
	// Get key and IV from a Base64String or any other ways.
	// You can generate a key and IV using "CryptoAesProvider.GenerateKey()"
	private readonly byte[] _encryptionKey = ...; 
	private readonly byte[] _encryptionIV = ...;
	private readonly IEncryptionCryptoProvider _provider;

	public DbSet<UserEntity> Users { get; set; }
	
	public DatabaseContext(DbContextOptions options)
		: base(options)
	{
		_provider = new CryptoAesProvider(this._encryptionKey, this._encryptionIV);
	}
	
	protected override void OnModelCreating(CryptoModelBuilder modelBuilder)
	{
		modelBuilder.UseEncryption(_provider);
	}
}
```

### Example with `CryptoAesProvider` and fluent configuration

```csharp
public class UserEntity
{
	public int Id { get; set; }
	public string Username { get; set; }
	public string Password { get; set; }
	public int Age { get; set; }
}

public class DatabaseContext : DbContext
{
	// Get key and IV from a Base64String or any other ways.
	// You can generate a key and IV using "CryptoAesProvider.GenerateKey()"
	private readonly byte[] _encryptionKey = ...; 
	private readonly byte[] _encryptionIV = ...;
	private readonly IEncryptionCryptoProvider _provider;

	public DbSet<UserEntity> Users { get; set; }
	
	public DatabaseContext(DbContextOptions options)
		: base(options)
	{
		_provider = new CryptoAesProvider(this._encryptionKey, this._encryptionIV);
	}
	
	protected override void OnModelCreating(CryptoModelBuilder modelBuilder)
	{
		// Entities builder *MUST* be called before UseEncryption().
		var userEntityBuilder = modelBuilder.Entity<UserEntity>();
		
		userEntityBuilder.Property(x => x.Username).IsRequired().IsEncrypted();
		userEntityBuilder.Property(x => x.Password).IsRequired().IsEncrypted();

		modelBuilder.UseEncryption(_provider);
	}
}
```

## Create an encryption provider

`EntityFrameworkCore.Crypto.DataEncryption` gives the possibility to create your own encryption providers. To do so, create a new class and make it inherit from `IEncryptionCryptoProvider`. You will need to implement the `Encrypt(string)` and `Decrypt(string)` methods.

```csharp
public class MyCustomEncryptionProvider : IEncryptionCryptoProvider
{
	public byte[] Encrypt(byte[] input)
	{
		// Encrypt the given input and return the encrypted data as a byte[].
	}
	
	public byte[] Decrypt(byte[] input)
	{
		// Decrypt the given input and return the decrypted data as a byte[].
	}
}
```

To use it, simply create a new `MyCustomEncryptionCryptoProvider` in your `DbContext` and pass it to the `UseEncryption` method:
```csharp
public class DatabaseContext : DbContext
{
	private readonly IEncryptionCryptoProvider _provider;

	public DatabaseContext(DbContextOptions options)
		: base(options)
	{
		_provider = new MyCustomEncryptionryptoCProvider();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseEncryption(_provider);
	}
}
```

## Thanks

Royce Lark would like to thank all the people that supports and contributes to the project and helped to improve the library. 

<h4 align="center"> If You Would Like To Buy Me A Coffee... You Can Donate Via Paypal: https://www.paypal.me/Mohang2 </h4><br>


