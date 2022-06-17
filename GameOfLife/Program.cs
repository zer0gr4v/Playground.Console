using System.Security.Cryptography;
using Spectre.Console;

namespace GameOfLife;

internal static class Program
{
    private static int _testers;
    private static int _developers;
    private static int _newFeatures;
    private static int _regressionBugs;
    private static int _productionIssues;
    private static int _matureFeatures;
    private static int _activeTeamMembers;

    public static void Main()
    {
        var instance = new GameOfLife();
        var grid = instance.GenerateGrid();
        var runSimulation = false;
        var stateCheck = 0;
        var outcome = Outcome.Win;

        Console.Clear();

        // Create a table
        var table = InitializeSpectreTable("Software development simulation game\nusing Conway's Game of Life",
            instance.Columns);

        AnsiConsole.Live(table)
            .AutoClear(false)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Top)
            .Start(ctx =>
            {
                Func<bool, Outcome> check = x =>
                    x ? Outcome.Win : Outcome.Lose;

                while (!runSimulation)
                {
                    var result = RunSimulation(instance, grid, table);
                    ctx.Refresh();
                    stateCheck += _activeTeamMembers == result ? 1 : -stateCheck;
                    _activeTeamMembers = result;
                    runSimulation = result == 0 || stateCheck == 5;

                    if (runSimulation)
                    {
                        outcome = check(stateCheck == 5);
                        break;
                    }

                    grid = instance.CreateNextGeneration(grid);
                    Thread.Sleep(500);
                }
            });

        GenerateEndGameMessage(outcome, RandomNumberGenerator.GetInt32(1000, 2000));
    }

    private static void GenerateEndGameMessage(Outcome outcome, int delay)
    {
        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Default)
            .Start($"[yellow] {(outcome == Outcome.Win ? "You won" : "You lose")}. " +
                   $"Project {(outcome == Outcome.Lose ? "was full of bugs" : "is handed over to support")}.[/]",
                ctx =>
                {
                    ExecuteAction(() => WriteLogMessage($"PBIs delivered: {_matureFeatures + _newFeatures}"), delay);
                    ExecuteAction(() => WriteLogMessage($"Active team members: {_activeTeamMembers}"), delay);
                    ExecuteAction(() => WriteLogMessage($"Developers hired: {_developers}"), delay);
                    ExecuteAction(() => WriteLogMessage($"Testers hired: {_testers}"), delay);
                    ExecuteAction(() => WriteLogMessage($"Regression bugs reported: {_regressionBugs}"), delay);
                    ExecuteAction(() => WriteLogMessage($"Production issues reported: {_productionIssues}"), delay);
                });
    }

    private static void WriteLogMessage(string message) =>
        AnsiConsole.MarkupLine($"[grey]LOG:[/] {message}[grey]...[/]");

    private static int RunSimulation(GameOfLife instance, GameOfLife.Cell[,] grid, Table table)
    {
        var rowValues = instance.RenderGrid(grid);
        var activeCells = 0;
        ExecuteAction(() =>
            rowValues.ToList().ForEach(x =>
            {
                if (table.Rows.Count > instance.Rows - 1)
                    table.RemoveRow(0);

                table.AddRow(x.RowItemArray);
                activeCells += x.ActiveCells;
                ClassifyCells(x.RowItemArray);
            })
        );

        return activeCells;
    }

    private static void ClassifyCells(IEnumerable<string> rowItemArray)
    {
        rowItemArray.GroupBy(x => x).ToList().ForEach(groupType =>
        {
            switch (groupType)
            {
                case {Key: Emoji.Known.NerdFace} or {Key: Emoji.Known.DisguisedFace}:
                    _developers += groupType.Count();
                    break;
                case {Key: Emoji.Known.FaceWithMonocle} or {Key: Emoji.Known.ThinkingFace}:
                    _testers += groupType.Count();
                    break;
                case {Key: Emoji.Known.Cockroach}:
                    _regressionBugs += groupType.Count();
                    break;
                case {Key: Emoji.Known.OkButton}:
                    _matureFeatures += groupType.Count();
                    break;
                case {Key: Emoji.Known.SosButton}:
                    _productionIssues += groupType.Count();
                    break;
                case {Key: Emoji.Known.NewButton}:
                    _newFeatures += groupType.Count();
                    break;
            }
        });
    }

    private static Table InitializeSpectreTable(string title, int columns)
    {
        var table = new Table
        {
            Border = TableBorder.Rounded,
            ShowHeaders = false,
            Title = new TableTitle(title)
        };

        for (var column = 0; column < columns; column++)
            table.AddColumn(column.ToString());

        return table;
    }

    private static void ExecuteAction(Action action, int delay = 0)
    {
        action();
        Thread.Sleep(delay);
    }
}