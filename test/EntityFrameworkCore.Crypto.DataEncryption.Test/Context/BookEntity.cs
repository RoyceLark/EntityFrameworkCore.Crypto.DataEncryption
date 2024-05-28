using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Microsoft.EntityFrameworkCore.DataEncryption.Test.Context;

public sealed class BookEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid UniqueId { get; set; }

    [Required]
    [CryptoEncrypted]
    public string Name { get; set; }

    [Required]
    public int NumberOfPages { get; set; }

    [Required]
    public int AuthorId { get; set; }

    [ForeignKey(nameof(AuthorId))]
    public AuthorEntity Author { get; set; }

    [CryptoEncrypted(CryptoStorageFormat.Base64)]
    [Column(TypeName = "TEXT")]
    public byte[] Content { get; set; }

    [CryptoEncrypted]
    [Column(TypeName = "BLOB")]
    public byte[] Summary { get; set; }

    public BookEntity(string name, int numberOfPages, byte[] content, byte[] summary)
    {
        Name = name;
        NumberOfPages = numberOfPages;
        UniqueId = Guid.NewGuid();
        Content = content;
        Summary = summary;
    }
}
