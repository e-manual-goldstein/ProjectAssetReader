using System;
using System.Collections.Generic;
using System.Linq;

public static class ConsoleTable
{
    private static List<string[]> _rows = new List<string[]>();
    private static char _delimiter = '\t';
    private static bool _showHorizontalLines = false;
    private static string[] _headers;

    public static bool ShowHorizontalLines
    {
        get { return _showHorizontalLines; }
        set { _showHorizontalLines = value; }
    }

    public static void AddLine(string line)
    {
        _rows.Add(line.Split(_delimiter));
    }

    public static void SetHeaders(string headerLine)
    {
        _headers = headerLine.Split(_delimiter);
        _rows.Clear();
    }

    public static void ShowOutput(char delimiter = '\t')
    {
        _delimiter = delimiter;

        if (_rows.Count == 0 && _headers == null)
        {
            Console.WriteLine("No data to display.");
            return;
        }

        int[] columnWidths = CalculateColumnWidths();
        string rowDelimiter = GenerateRowDelimiter(columnWidths);

        if (_headers != null)
        {
            Console.WriteLine(rowDelimiter);
            PrintRow(_headers, columnWidths);
            Console.WriteLine(rowDelimiter);
        }

        foreach (var row in _rows)
        {
            PrintRow(row, columnWidths);
            if (_showHorizontalLines)
                Console.WriteLine(rowDelimiter);
        }

        _rows.Clear(); // Clearing the buffer after printing the output
    }

    private static void PrintRow(string[] row, int[] columnWidths)
    {
        string rowString = "|";
        for (int i = 0; i < row.Length; i++)
        {
            rowString += row[i].PadRight(columnWidths[i]) + "|";
        }
        Console.WriteLine(rowString);
    }

    private static int[] CalculateColumnWidths()
    {
        int[] widths = new int[_rows.Max(x => x.Length)];

        for (int i = 0; i < widths.Length; i++)
        {
            int headerWidth = _headers != null && _headers.Length > i ? _headers[i].Length : 0;
            int maxRowWidth = _rows.Max(x => x.Length > i ? x[i].Length : 0);
            widths[i] = Math.Max(headerWidth, maxRowWidth) + 2;
        }

        return widths;
    }

    private static string GenerateRowDelimiter(int[] columnWidths)
    {
        string rowDelimiter = "+";
        foreach (int width in columnWidths)
        {
            rowDelimiter += new string('-', width) + "+";
        }
        return rowDelimiter;
    }
}
