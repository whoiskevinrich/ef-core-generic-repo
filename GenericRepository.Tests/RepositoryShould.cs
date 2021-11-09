using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace GenericRepository.Tests
{
    public class GenericRepositoryTests
    {
        [Fact] 
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            await using var context = BuildTestObjectContext();
            await context.AddAsync(new TestObject {FirstName = "Luke", LastName = "Skywalker"});
            await context.AddAsync(new TestObject {FirstName = "Han", LastName = "Solo"});
            await context.SaveChangesAsync();

            var sut = new GenericRepository<TestObject>(context);

            var results = await sut.GetAllAsync();
            results.ShouldSatisfyAllConditions(
                () => results.Count().ShouldBe(2),
                () => results.Count(x => x.FirstName == "Luke").ShouldBe(1),
                () => results.Count(x => x.FirstName == "Han").ShouldBe(1));
        }

        [Fact]
        public async Task InsertAsync_ShouldGenerateId()
        {
            await using var context = BuildTestObjectContext();
            var sut = new GenericRepository<TestObject>(context);
            var testEntity = new TestObject{FirstName = "Leia"};

            await sut.InsertAsync(testEntity);
            
            context.ShouldSatisfyAllConditions(
                () => context.Set<TestObject>().Count(x => x.FirstName == testEntity.FirstName).ShouldBe(1),
                () => context.Set<TestObject>().First(x => x.FirstName == testEntity.FirstName)
                    .PersonId.ShouldBeOfType(typeof(Guid)));
        }
        
        [Fact]
        public async Task FindAsync_ShouldReturnObjectById()
        {
            await using var context = BuildTestObjectContext();
            var sut = new GenericRepository<TestObject>(context);
            var personName = "Chewbacca";
            var testObject = new TestObject{FirstName = personName};
            var testEntity = await context.AddAsync(testObject);

            var result = await sut.FindAsync(testEntity.Entity.PersonId);

            result.FirstName.ShouldBe(personName);
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntityProperties()
        {
            await using var context = BuildTestObjectContext();
            var sut = new GenericRepository<TestObject>(context);
            var anakin = new TestObject
                {FirstName = "Anakin", LastName = "Skywalker", Email = "anakinskywalker@jedi.net"};
            var vader = new TestObject
                {FirstName = "Darth", LastName = "Vader", Email = "lordvader@sith.com"};
            var entityEntry = await context.AddAsync(anakin);
            await context.SaveChangesAsync();
            var entity = entityEntry.Entity;
            
            // validate state in context is as the test expected
            context.ShouldSatisfyAllConditions(
                () => context.TestObjects.ShouldHaveSingleItem(),
                () => context.TestObjects.Single().FirstName.ShouldBe(anakin.FirstName),
                () => context.TestObjects.Single().LastName.ShouldBe(anakin.LastName),
                () => context.TestObjects.Single().Email.ShouldBe(anakin.Email));

            // update the local working copy of the entity
            entity.Email = vader.Email;
            entity.FirstName = vader.FirstName;
            entity.LastName = vader.LastName;
            await sut.UpdateAsync(entity);
            
            // validate state was updated
            context.ShouldSatisfyAllConditions(
                () => context.TestObjects.ShouldHaveSingleItem(),
                () => context.TestObjects.Single().FirstName.ShouldBe(vader.FirstName),
                () => context.TestObjects.Single().LastName.ShouldBe(vader.LastName),
                () => context.TestObjects.Single().Email.ShouldBe(vader.Email));
        }

        [Fact] 
        public async Task DeleteAsync_ShouldRemoveEntry()
        {
            await using var context = BuildTestObjectContext();
            var sut = new GenericRepository<TestObject>(context);
            var person = new TestObject{FirstName = "Ben", LastName = "Kenobi"};
            await context.AddAsync(person);
            await context.SaveChangesAsync();
            
            // validate test setup
            context.TestObjects.ShouldHaveSingleItem();

            await sut.DeleteAsync(person);

            context.TestObjects.ShouldBeEmpty();
        }
        

        private static TestObjectContext BuildTestObjectContext()
        {
            var options = new DbContextOptionsBuilder()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new TestObjectContext(options);
        }
    }
}