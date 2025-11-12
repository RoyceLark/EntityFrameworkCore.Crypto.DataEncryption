using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Internal;

/// <summary>
/// Defines the internal encryption converter for string values.
/// </summary>
/// <typeparam name="TModel"></typeparam>
/// <typeparam name="TProvider"></typeparam>
internal sealed class CryptoEncryptionConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
{
    /// <summary>
    /// Creates a new <see cref="CryptoEncryptionConverter{TModel,TProvider}"/> instance.
    /// </summary>
    /// <param name="encryptionProvider">Encryption provider to use.</param>
    /// <param name="storageFormat">Encryption storage format.</param>
    /// <param name="mappingHints">Mapping hints.</param>
    public CryptoEncryptionConverter(IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat, ConverterMappingHints? mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, storageFormat), 
            mappingHints)
    {
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat)
    {
        byte[]? inputData = input switch
        {
            string s => !string.IsNullOrEmpty(s) ? Encoding.UTF8.GetBytes(s) : null,
            byte[] b => b,
            _ => null,
        };

        byte[] encryptedRawBytes = encryptionProvider.Encrypt(inputData ?? Array.Empty<byte>());

        if (encryptedRawBytes is null)
        {
            // Return default value for TOutput to avoid possible null reference return
            return default!;
        }

        object encryptedData = storageFormat switch
        {
            CryptoStorageFormat.Default or CryptoStorageFormat.Base64 => Convert.ToBase64String(encryptedRawBytes),
            _ => encryptedRawBytes
        };

        // Ensure that Convert.ChangeType never returns null by using default if result is null
        var result = Convert.ChangeType(encryptedData, typeof(TOutput));
        return result is not null ? (TOutput)result : default!;
    }

    private static TModel Decrypt<TInput, TOupout>(TProvider input, IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat)
    {
        Type destinationType = typeof(TModel);
        byte[]? inputData = storageFormat switch
        {
            CryptoStorageFormat.Default or CryptoStorageFormat.Base64 => 
                input != null && input.ToString() != null
                    ? Convert.FromBase64String(input.ToString()!)
                    : Array.Empty<byte>(),
            _ => input as byte[] ?? Array.Empty<byte>()
        };
        byte[] decryptedRawBytes = encryptionProvider.Decrypt(inputData);
        object? decryptedData = null;

        if (decryptedRawBytes != null && destinationType == typeof(string))
        {
            decryptedData = Encoding.UTF8.GetString(decryptedRawBytes).Trim('\0');
        }
        else if (destinationType == typeof(byte[]))
        {
            decryptedData = decryptedRawBytes;
        }

        // Fix CS8600 and CS8603 by handling possible nulls
        if (decryptedData is null)
        {
            // If TModel is a reference type, return null; if value type, return default
            return default!;
        }

        return (TModel)Convert.ChangeType(decryptedData, typeof(TModel));
    }
}
