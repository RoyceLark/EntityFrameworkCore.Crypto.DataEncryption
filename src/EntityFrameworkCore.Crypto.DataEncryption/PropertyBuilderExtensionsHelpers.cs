
using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.DataEncryption
{
    internal static class PropertyBuilderExtensionsHelpers
    {
        public static PropertyBuilder<TProperty> IsEncrypted<TProperty>(this PropertyBuilder<TProperty> builder, CryptoStorageFormat storageFormat = CryptoStorageFormat.Default)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasAnnotation(CryptoPropertyAnnotations.IsEncrypted, true);
            builder.HasAnnotation(CryptoPropertyAnnotations.StorageFormat, storageFormat);

            return builder;
        }
    }
}