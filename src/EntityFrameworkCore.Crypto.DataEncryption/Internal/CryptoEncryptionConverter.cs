﻿using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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
    public CryptoEncryptionConverter(IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat, ConverterMappingHints mappingHints = null)
        : base(
            x => Encrypt<TModel, TProvider>(x, encryptionProvider, storageFormat),
            x => Decrypt<TModel, TProvider>(x, encryptionProvider, storageFormat), 
            mappingHints)
    {
    }

    private static TOutput Encrypt<TInput, TOutput>(TInput input, IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat)
    {
        byte[] inputData = input switch
        {
            string => !string.IsNullOrEmpty(input.ToString()) ? Encoding.UTF8.GetBytes(input.ToString()) : null,
            byte[] => input as byte[],
            _ => null,
        };

        byte[] encryptedRawBytes = encryptionProvider.Encrypt(inputData);

        if (encryptedRawBytes is null)
        {
            return default;
        }

        object encryptedData = storageFormat switch
        {
            CryptoStorageFormat.Default or CryptoStorageFormat.Base64 => Convert.ToBase64String(encryptedRawBytes),
            _ => encryptedRawBytes
        };

        return (TOutput)Convert.ChangeType(encryptedData, typeof(TOutput));
    }

    private static TModel Decrypt<TInput, TOupout>(TProvider input, IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat)
    {
        Type destinationType = typeof(TModel);
        byte[] inputData = storageFormat switch
        {
            CryptoStorageFormat.Default or CryptoStorageFormat.Base64 => Convert.FromBase64String(input.ToString()),
            _ => input as byte[]
        };
        byte[] decryptedRawBytes = encryptionProvider.Decrypt(inputData);
        object decryptedData = null;

        if (decryptedRawBytes != null && destinationType == typeof(string))
        {
            decryptedData = Encoding.UTF8.GetString(decryptedRawBytes).Trim('\0');
        }
        else if (destinationType == typeof(byte[]))
        {
            decryptedData = decryptedRawBytes;
        }

        return (TModel)Convert.ChangeType(decryptedData, typeof(TModel));
    }
}
