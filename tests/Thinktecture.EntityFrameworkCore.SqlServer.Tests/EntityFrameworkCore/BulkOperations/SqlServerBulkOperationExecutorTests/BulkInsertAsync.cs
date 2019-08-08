using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Moq;
using Thinktecture.EntityFrameworkCore.TempTables;
using Thinktecture.TestDatabaseContext;
using Xunit;
using Xunit.Abstractions;

namespace Thinktecture.EntityFrameworkCore.BulkOperations.SqlServerBulkOperationExecutorTests
{
   // ReSharper disable once InconsistentNaming
   public class BulkInsertAsync : IntegrationTestsBase
   {
      private readonly SqlServerBulkOperationExecutor _sut;

      public BulkInsertAsync([NotNull] ITestOutputHelper testOutputHelper)
         : base(testOutputHelper, true)
      {
         var sqlGenerationHelperMock = new Mock<ISqlGenerationHelper>();
         sqlGenerationHelperMock.Setup(h => h.DelimitIdentifier(It.IsAny<string>(), It.IsAny<string>()))
                                .Returns<string, string>((name, schema) => schema == null ? $"[{name}]" : $"[{schema}].[{name}]");

         var logger = CreateDiagnosticsLogger<SqlServerDbLoggerCategory.BulkOperation>();
         _sut = new SqlServerBulkOperationExecutor(sqlGenerationHelperMock.Object, logger);
      }

      [Fact]
      public void Should_throw_when_inserting_queryType_without_providing_tablename()
      {
         ConfigureModel = builder => builder.ConfigureTempTable<int>();

         _sut.Awaiting(sut => sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TempTable<int>>(), new List<TempTable<int>>(), new SqlBulkInsertOptions()))
             .Should().Throw<InvalidOperationException>();
      }

      [Fact]
      public async Task Should_insert_entities()
      {
         var testEntity = new TestEntity
                          {
                             Id = new Guid("40B5CA93-5C02-48AD-B8A1-12BC13313866"),
                             Name = "Name",
                             Count = 42
                          };

         var testEntities = new[] { testEntity };

         await _sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TestEntity>(), testEntities, new SqlBulkInsertOptions());

         var loadedEntities = await AssertDbContext.TestEntities.ToListAsync();
         loadedEntities.Should().HaveCount(1)
                       .And.Subject.First()
                       .Should().BeEquivalentTo(new TestEntity
                                                {
                                                   Id = new Guid("40B5CA93-5C02-48AD-B8A1-12BC13313866"),
                                                   Name = "Name",
                                                   Count = 42
                                                });
      }

      [Fact]
      public async Task Should_insert_private_property()
      {
         var testEntity = new TestEntity { Id = new Guid("40B5CA93-5C02-48AD-B8A1-12BC13313866") };
         testEntity.SetPrivateField(3);

         var testEntities = new[] { testEntity };

         await _sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TestEntity>(), testEntities, new SqlBulkInsertOptions());

         var loadedEntity = await AssertDbContext.TestEntities.FirstOrDefaultAsync();
         loadedEntity.GetPrivateField().Should().Be(3);
      }

      [Fact]
      public async Task Should_ignore_shadow_property()
      {
         var testEntity = new TestEntity { Id = new Guid("40B5CA93-5C02-48AD-B8A1-12BC13313866") };
         ActDbContext.Entry(testEntity).Property("ShadowProperty").CurrentValue = "value";

         var testEntities = new[] { testEntity };

         await _sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TestEntity>(), testEntities, new SqlBulkInsertOptions());

         var loadedEntity = await AssertDbContext.TestEntities.FirstOrDefaultAsync();
         AssertDbContext.Entry(loadedEntity).Property("ShadowProperty").CurrentValue.Should().BeNull();
      }

      [Fact]
      public async Task Should_insert_auto_increment_column_with_KeepIdentity()
      {
         var testEntity = new TestEntityWithAutoIncrement { Id = 42 };
         var testEntities = new[] { testEntity };

         var options = new SqlBulkInsertOptions { SqlBulkCopyOptions = SqlBulkCopyOptions.KeepIdentity };
         await _sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TestEntityWithAutoIncrement>(), testEntities, options);

         var loadedEntity = await AssertDbContext.TestEntitiesWithAutoIncrement.FirstOrDefaultAsync();
         loadedEntity.Id.Should().Be(42);
      }

      [Fact]
      public async Task Should_ignore_auto_increment_column_without_KeepIdentity()
      {
         var testEntity = new TestEntityWithAutoIncrement { Id = 42, Name = "value" };
         var testEntities = new[] { testEntity };

         await _sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TestEntityWithAutoIncrement>(), testEntities, new SqlBulkInsertOptions());

         var loadedEntity = await AssertDbContext.TestEntitiesWithAutoIncrement.FirstOrDefaultAsync();
         loadedEntity.Id.Should().NotBe(0);
         loadedEntity.Name.Should().Be("value");
      }

      [Fact]
      public async Task Should_ignore_RowVersion()
      {
         var testEntity = new TestEntityWithRowVersion { Id = new Guid("EBC95620-4D80-4318-9B92-AD7528B2965C"), RowVersion = Int32.MaxValue };
         var testEntities = new[] { testEntity };

         await _sut.BulkInsertAsync(ActDbContext, ActDbContext.GetEntityType<TestEntityWithRowVersion>(), testEntities, new SqlBulkInsertOptions());

         var loadedEntity = await AssertDbContext.TestEntitiesWithRowVersion.FirstOrDefaultAsync();
         loadedEntity.Id.Should().Be(new Guid("EBC95620-4D80-4318-9B92-AD7528B2965C"));
         loadedEntity.RowVersion.Should().NotBe(Int32.MaxValue);
      }

      [Fact]
      public async Task Should_insert_specified_properties_only()
      {
         var testEntity = new TestEntity
                          {
                             Id = new Guid("40B5CA93-5C02-48AD-B8A1-12BC13313866"),
                             Name = "Name",
                             Count = 42,
                             PropertyWithBackingField = 7
                          };
         testEntity.SetPrivateField(3);
         var testEntities = new[] { testEntity };
         var idProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Id));
         var countProperty = typeof(TestEntity).GetProperty(nameof(TestEntity.Count));
         var propertyWithBackingField = typeof(TestEntity).GetProperty(nameof(TestEntity.PropertyWithBackingField));
         var privateField = typeof(TestEntity).GetField("_privateField", BindingFlags.Instance | BindingFlags.NonPublic);

         await _sut.BulkInsertAsync(ActDbContext,
                                    ActDbContext.GetEntityType<TestEntity>(),
                                    testEntities,
                                    new SqlBulkInsertOptions
                                    {
                                       EntityMembersProvider = new EntityMembersProvider(new MemberInfo[]
                                                                                         {
                                                                                            idProperty,
                                                                                            countProperty,
                                                                                            propertyWithBackingField,
                                                                                            privateField
                                                                                         })
                                    });

         var loadedEntities = await AssertDbContext.TestEntities.ToListAsync();
         loadedEntities.Should().HaveCount(1);
         var loadedEntity = loadedEntities[0];
         loadedEntity.Should().BeEquivalentTo(new TestEntity
                                              {
                                                 Id = new Guid("40B5CA93-5C02-48AD-B8A1-12BC13313866"),
                                                 Count = 42,
                                                 PropertyWithBackingField = 7
                                              });
         loadedEntity.GetPrivateField().Should().Be(3);
      }
   }
}
