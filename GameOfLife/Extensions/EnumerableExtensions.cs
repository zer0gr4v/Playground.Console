namespace GameOfLife.Extensions;

internal static class EnumerableExtensions
{
    private static IEnumerable<T> SliceRow<T>(this T[,] @this, int row)
    {
        for (var i = @this.GetLowerBound(1); i <= @this.GetUpperBound(1); i++)
            yield return @this[row, i];
    }

    public static IEnumerable<T> SliceColumn<T>(this T[,] @this, int column)
    {
        for (var i = @this.GetLowerBound(0); i <= @this.GetUpperBound(0); i++)
            yield return @this[i, column];
    }
}