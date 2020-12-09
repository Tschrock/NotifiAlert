using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NotifiAlert
{
    /// <summary>
    /// Code modified from https://stackoverflow.com/a/19353995
    /// </summary>
    public static class TableParser
    {
        public static string ToStringTable<T>(this T[] values, string[] headers, params Func<T, object>[] selectors)
        {
            Debug.Assert(headers.Length == selectors.Length);

            var tableValues = new string[values.Length + 1, selectors.Length];

            // Fill headers
            for (int colIndex = 0; colIndex < selectors.Length; colIndex++)
            {
                tableValues[0, colIndex] = headers[colIndex];
            }

            // Fill table rows
            for (int rowIndex = 0; rowIndex < values.Length; rowIndex++)
            {
                for (int colIndex = 0; colIndex < selectors.Length; colIndex++)
                {
                    tableValues[rowIndex + 1, colIndex] = selectors[colIndex](values[rowIndex])?.ToString() ?? "<null>";
                }
            }

            return ToStringTable(tableValues);
        }

        public static string ToStringTable(this string[,] values)
        {
            int rows = values.GetLength(0);
            int cols = values.GetLength(1);
            int[] columnWidths = GetMaxColumnsWidth(values);
            var sb = new StringBuilder();
            
            sb.AppendTableSplitter(columnWidths);
            for (int rowIndex = 0; rowIndex < rows; rowIndex++)
            {
                sb.Append("| ");
                for (int colIndex = 0; colIndex < cols; colIndex++)
                {
                    if (colIndex != 0) sb.Append(" | ");
                    sb.Append(values[rowIndex, colIndex].PadRight(columnWidths[colIndex]));
                }
                sb.Append(" |");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0) sb.AppendTableSplitter(columnWidths);
            }
            sb.AppendTableSplitter(columnWidths);

            return sb.ToString();
        }

        private static void AppendTableSplitter(this StringBuilder sb, int[] columnWidths) {
            sb.Append("+");
            foreach (int width in columnWidths)
            {
                sb.Append(new string('-', width + 2));
                sb.Append("+");
            }
            sb.AppendLine();
        }

        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    int newLength = arrValues[rowIndex, colIndex].Length;
                    int oldLength = maxColumnsWidth[colIndex];

                    if (newLength > oldLength)
                    {
                        maxColumnsWidth[colIndex] = newLength;
                    }
                }
            }

            return maxColumnsWidth;
        }
    }
}