using System.Security.Cryptography;
using Spectre.Console;

namespace GameOfLife;

internal class GameOfLife
{
    public enum Cell
    {
        Inactive,
        Active
    }

    internal int Rows { get; }
    internal int Columns { get; }
    internal string[] ActiveCellEmojis { get; }
    internal string[] InactiveCellEmojis { get; }
    
    public GameOfLife(int rows = 10, int columns = 10)
    {
        Rows = rows;
        Columns = columns;
        ActiveCellEmojis = new[]
            {Emoji.Known.ThinkingFace, Emoji.Known.DisguisedFace, Emoji.Known.NerdFace, Emoji.Known.FaceWithMonocle};
        InactiveCellEmojis = new[]
            {Emoji.Known.Cockroach, Emoji.Known.OkButton, Emoji.Known.SosButton, Emoji.Known.NewButton};
    }

    internal Cell[,] GenerateGrid()
    {
        var grid = new Cell[Rows, Columns];
        for (var row = 0; row < Rows; row++)
        for (var column = 0; column < Columns; column++)
            grid[row, column] = (Cell) RandomNumberGenerator.GetInt32(0, 2);

        return grid;
    }

    internal Cell[,] CreateNextGeneration(Cell[,] currentGrid)
    {
        var nextGeneration = new Cell[Rows, Columns];

        // Loop through every cell 
        for (var row = 1; row < Rows - 1; row++)
        for (var column = 1; column < Columns - 1; column++)
        {
            // find your active neighbors
            var activeCellNeighbours = FindActiveCellNeighbours(row, column, currentGrid);
            var currentCell = currentGrid[row, column];

            // The cell needs to be subtracted 
            // from its neighbours as it was counted before 
            activeCellNeighbours -= currentCell == Cell.Active ? 1 : 0;

            // Implementing the Rules of Life
            nextGeneration[row, column] = true switch
            {
                // Cell is lonely and dies
                _ when currentCell.HasFlag(Cell.Active) && activeCellNeighbours < 2 => Cell.Inactive,
                // Cell dies due to over population
                _ when currentCell.HasFlag(Cell.Active) && activeCellNeighbours > 3 => Cell.Inactive,
                // A new cell is born 
                _ when currentCell.HasFlag(Cell.Inactive) && activeCellNeighbours == 3 => Cell.Active,
                _ => currentCell
            };
        }

        return nextGeneration;
    }

    private int FindActiveCellNeighbours(int row, int column, Cell[,] grid)
    {
        var activeCellNeighbours = 0;
        for (var i = -1; i <= 1; i++)
        for (var j = -1; j <= 1; j++)
            activeCellNeighbours += grid[row + i, column + j] == Cell.Active ? 1 : 0;
        return activeCellNeighbours;
    }

    internal IEnumerable<(string[] RowItemArray, int ActiveCells)> RenderGrid(Cell[,] grid)
    {
        for (var row = 0; row < Rows; row++)
        {
            var activeCells = 0;
            var rowItemArray = new string[Columns];
            for (var column = 0; column < Columns; column++)
            {
                var cell = grid[row, column];
                rowItemArray[column] =
                    cell == Cell.Active ? PickRandomEmoji(Cell.Active) : PickRandomEmoji(Cell.Inactive);
                activeCells += (int) cell;
            }

            yield return (rowItemArray, activeCells);
        }
    }

    private string PickRandomEmoji(Cell cell)
    {
        return cell switch
        {
            Cell.Active => ActiveCellEmojis.ElementAt(
                RandomNumberGenerator.GetInt32(0, ActiveCellEmojis.Length)),
            Cell.Inactive => InactiveCellEmojis.ElementAt(
                RandomNumberGenerator.GetInt32(0, InactiveCellEmojis.Length)),
            _ => throw new ArgumentOutOfRangeException(nameof(cell), cell, null)
        };
    }
}