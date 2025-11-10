using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;

namespace Thinktecture.EntityFrameworkCore.Query;

/// <inheritdoc />
[SuppressMessage("Usage", "EF1001", MessageId = "Internal EF Core API usage.")]
public class ThinktectureNpgsqlParameterBasedSqlProcessor : NpgsqlParameterBasedSqlProcessor
{
   /// <inheritdoc />
   public ThinktectureNpgsqlParameterBasedSqlProcessor(
      RelationalParameterBasedSqlProcessorDependencies dependencies,
      RelationalParameterBasedSqlProcessorParameters parameters)
      : base(dependencies, parameters)
   {
   }
}
