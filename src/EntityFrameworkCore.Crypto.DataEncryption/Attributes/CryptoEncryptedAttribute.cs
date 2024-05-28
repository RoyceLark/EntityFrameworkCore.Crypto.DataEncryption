namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Specifies that the data field value should be encrypted.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class CryptoEncryptedAttribute : Attribute
{
    /// <summary>
    /// Returns the storage format for the database value.
    /// </summary>
    public CryptoStorageFormat Format { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoEncryptedAttribute"/> class.
    /// </summary>
    /// <param name="format">
    /// The storage format.
    /// </param>
    public CryptoEncryptedAttribute(CryptoStorageFormat format)
    {
        Format = format;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoEncryptedAttribute"/> class.
    /// </summary>
    public CryptoEncryptedAttribute() 
        : this(CryptoStorageFormat.Default)
    {
    }
}