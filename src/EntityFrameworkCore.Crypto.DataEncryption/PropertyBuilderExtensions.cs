using Microsoft.EntityFrameworkCore.DataEncryption.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.EntityFrameworkCore.DataEncryption;

/// <summary>
/// Provides extensions for the <see cref="PropertyBuilder"/> type.
/// </summary>
public static class PropertyBuilderExtensions
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static PropertyBuilder<TProperty> IsEncrypted<TProperty>(this PropertyBuilder<TProperty> builder, CryptoStorageFormat storageFormat = CryptoStorageFormat.Default)
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
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
