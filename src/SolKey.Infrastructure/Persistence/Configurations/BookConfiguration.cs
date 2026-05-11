using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SolKey.Domain.Entities;

namespace SolKey.Infrastructure.Persistence.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.Property(book => book.Title).HasMaxLength(200).IsRequired();
        builder.Property(book => book.Description).HasMaxLength(2000).IsRequired();
        builder.Property(book => book.CoverImage).HasMaxLength(500).IsRequired();
        builder.HasMany(book => book.Chapters)
            .WithOne(chapter => chapter.Book)
            .HasForeignKey(chapter => chapter.BookId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasQueryFilter(book => !book.IsDeleted);
    }
}
