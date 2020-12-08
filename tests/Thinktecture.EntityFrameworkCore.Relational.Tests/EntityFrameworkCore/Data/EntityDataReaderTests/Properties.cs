using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Thinktecture.TestDatabaseContext;
using Xunit;
using Xunit.Abstractions;

namespace Thinktecture.EntityFrameworkCore.Data.EntityDataReaderTests
{
   public class Properties : IntegrationTestsBase
   {
      private readonly List<IProperty> _propertiesToRead = new();
      private readonly IProperty _column1;
      private readonly IProperty _column2;

      private EntityDataReader<TestEntity>? _sut;

      // ReSharper disable once InconsistentNaming
      private EntityDataReader<TestEntity> SUT => _sut ??= new EntityDataReader<TestEntity>(LoggerFactory!.CreateLogger("EntityDataReader"), ActDbContext, Array.Empty<TestEntity>(), _propertiesToRead);

      public Properties(ITestOutputHelper testOutputHelper)
         : base(testOutputHelper)
      {
         _column1 = ArrangeDbContext.GetEntityType<TestEntity>().GetProperty(nameof(TestEntity.Column1));
         _column2 = ArrangeDbContext.GetEntityType<TestEntity>().GetProperty(nameof(TestEntity.Column2));
      }

      [Fact]
      public void Should_return_propertiesToRead()
      {
         _propertiesToRead.Add(_column1);
         _propertiesToRead.Add(_column2);

         SUT.Properties.Should().HaveCount(2);
         SUT.Properties.Should().Contain(_column1);
         SUT.Properties.Should().Contain(_column2);
      }

      protected override void Dispose(bool disposing)
      {
         _sut?.Dispose();

         base.Dispose(disposing);
      }
   }
}
