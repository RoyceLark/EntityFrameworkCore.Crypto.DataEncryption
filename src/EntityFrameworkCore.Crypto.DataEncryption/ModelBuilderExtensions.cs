using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.DataEncryption;

/// <summary>
/// Provides extensions for the <see cref="ModelBuilder"/>.
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Enables encryption on this model using an encryption provider.
    /// </summary>
    /// <param name="modelBuilder">
    /// The <see cref="ModelBuilder"/> instance.
    /// </param>
    /// <param name="encryptionProvider">
    /// The <see cref="IEncryptionCryptoProvider"/> to use, if any.
    /// </param>
    /// <returns>
    /// The updated <paramref name="modelBuilder"/>.
    /// </returns>
    public static ModelBuilder UseEncryption(this ModelBuilder modelBuilder, IEncryptionCryptoProvider encryptionProvider)
    {
        if (modelBuilder is null)
        {
            throw new ArgumentNullException(nameof(modelBuilder));
        }

        if (encryptionProvider is null)
        {
            throw new ArgumentNullException(nameof(encryptionProvider));
        }

        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            IEnumerable<EncryptedProperty> encryptedProperties = GetEntityEncryptedProperties(entityType);

            foreach (EncryptedProperty encryptedProperty in encryptedProperties)
            {
#pragma warning disable EF1001 // Internal EF Core API usage.
                if (encryptedProperty.Property.FindAnnotation(CoreAnnotationNames.ValueConverter) is not null)
                {
                    continue;
                }
#pragma warning restore EF1001 // Internal EF Core API usage.

                ValueConverter converter = GetValueConverter(encryptedProperty.Property.ClrType, encryptionProvider, encryptedProperty.StorageFormat);

                if (converter != null)
                {
                    encryptedProperty.Property.SetValueConverter(converter);
                }
            }
        }

        return modelBuilder;
    }

    private static ValueConverter GetValueConverter(Type propertyType, IEncryptionCryptoProvider encryptionProvider, CryptoStorageFormat storageFormat)
    {
        if (propertyType == typeof(string))
        {
            return storageFormat switch
            {
                CryptoStorageFormat.Default or CryptoStorageFormat.Base64 => new CryptoEncryptionConverter<string, string>(encryptionProvider, CryptoStorageFormat.Base64),
                CryptoStorageFormat.Binary => new CryptoEncryptionConverter<string, byte[]>(encryptionProvider, CryptoStorageFormat.Binary),
                _ => throw new NotImplementedException()
            };
        }
        else if (propertyType == typeof(byte[]))
        {
            return storageFormat switch
            {
                CryptoStorageFormat.Default or CryptoStorageFormat.Binary => new CryptoEncryptionConverter<byte[], byte[]>(encryptionProvider, CryptoStorageFormat.Binary),
                CryptoStorageFormat.Base64 => new CryptoEncryptionConverter<byte[], string>(encryptionProvider, CryptoStorageFormat.Base64),
                _ => throw new NotImplementedException()
            };
        }

        throw new NotImplementedException($"Type {propertyType.Name} does not support encryption.");
    }

    private static IEnumerable<EncryptedProperty> GetEntityEncryptedProperties(IMutableEntityType entity)
    {
        return entity.GetProperties()
            .Select(x => EncryptedProperty.Create(x))
            .Where(x => x is not null);
    }

    internal class EncryptedProperty
    {
        public IMutableProperty Property { get; }

        public CryptoStorageFormat StorageFormat { get; }

        private EncryptedProperty(IMutableProperty property, CryptoStorageFormat storageFormat)
        {
            Property = property;
            StorageFormat = storageFormat;
        }

        public static EncryptedProperty Create(IMutableProperty property)
        {
            CryptoStorageFormat? storageFormat = null;

            var encryptedAttribute = property.PropertyInfo?.GetCustomAttribute<CryptoEncryptedAttribute>(false);

            if (encryptedAttribute != null)
            {
                storageFormat = encryptedAttribute.Format;
            }

            IAnnotation encryptedAnnotation = property.FindAnnotation(CryptoPropertyAnnotations.IsEncrypted);

            if (encryptedAnnotation != null && (bool)encryptedAnnotation.Value)
            {
                storageFormat = (CryptoStorageFormat)property.FindAnnotation(CryptoPropertyAnnotations.StorageFormat)?.Value;
            }

            return storageFormat.HasValue ? new EncryptedProperty(property, storageFormat.Value) : null;
        }
    }
}
