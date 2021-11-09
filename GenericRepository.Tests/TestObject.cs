using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericRepository.Tests
{
    public class TestObject
    {
        public Guid PersonId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class TestObjectConfiguration : IEntityTypeConfiguration<TestObject>
    {
        public void Configure(EntityTypeBuilder<TestObject> entity)
        {
            entity.HasKey(x => x.PersonId);
        }
    }

    public class TestObjectContext : DbContext
    {
        public TestObjectContext(DbContextOptions options) : base(options) { }
        
        public DbSet<TestObject> TestObjects { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder) => 
            builder.ApplyConfigurationsFromAssembly(typeof(TestObjectContext).Assembly);
    }
}