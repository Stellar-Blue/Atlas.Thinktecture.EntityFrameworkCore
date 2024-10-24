using System.Linq.Expressions;

// ReSharper disable once CheckNamespace
namespace Thinktecture;

/// <summary>
/// Extension methods for <see cref="ExpressionVisitor"/>.
/// </summary>
public static class RelationalExpressionVisitorExtensions
{
   /// <summary>
   /// Visits a collection of <paramref name="expressions"/> and returns new collection if it least one expression has been changed.
   /// Otherwise the provided <paramref name="expressions"/> are returned if there are no changes.
   /// </summary>
   /// <param name="visitor">Visitor to use.</param>
   /// <param name="expressions">Expressions to visit.</param>
   /// <returns>
   /// New collection with visited expressions if at least one visited expression has been changed; otherwise the provided <paramref name="expressions"/>.
   /// </returns>
   public static IReadOnlyList<T> VisitExpressions<T>(this ExpressionVisitor visitor, IReadOnlyList<T> expressions)
      where T : Expression
   {
      ArgumentNullException.ThrowIfNull(visitor);
      ArgumentNullException.ThrowIfNull(expressions);

      var visitedExpressions = new List<T>();
      var hasChanges = false;

      foreach (var expression in expressions)
      {
         var visitedExpression = (T)visitor.Visit(expression);
         visitedExpressions.Add(visitedExpression);
         hasChanges |= !ReferenceEquals(visitedExpression, expression);
      }

      return hasChanges ? visitedExpressions.AsReadOnly() : expressions;
   }
}
