using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

namespace Thinktecture.EntityFrameworkCore.Query;

/// <summary>
/// Extends <see cref="RelationalParameterBasedSqlProcessor"/>.
/// </summary>
[SuppressMessage("Usage", "EF1001:Internal EF Core API usage.")]
public class ThinktectureSqliteParameterBasedSqlProcessor : SqliteParameterBasedSqlProcessor
{
   /// <inheritdoc />
   public ThinktectureSqliteParameterBasedSqlProcessor(
      RelationalParameterBasedSqlProcessorDependencies dependencies,
      RelationalParameterBasedSqlProcessorParameters parameters)
      : base(dependencies, parameters)
   {
   }
}
