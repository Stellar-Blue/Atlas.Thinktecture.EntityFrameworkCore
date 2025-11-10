using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

namespace Thinktecture.EntityFrameworkCore.Query;

/// <inheritdoc />
[SuppressMessage("Usage", "EF1001", MessageId = "Internal EF Core API usage.")]
public class ThinktectureSqlServerParameterBasedSqlProcessor : SqlServerParameterBasedSqlProcessor
{
   /// <inheritdoc />
   public ThinktectureSqlServerParameterBasedSqlProcessor(
      RelationalParameterBasedSqlProcessorDependencies dependencies,
      RelationalParameterBasedSqlProcessorParameters parameters,
      ISqlServerSingletonOptions sqlServerSingletonOptions)
      : base(dependencies, parameters, sqlServerSingletonOptions)
   {
   }
}
