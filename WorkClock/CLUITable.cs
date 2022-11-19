using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkClock
{
    /// <summary>
    /// Represents a table that can be printed to a command line interface
    /// </summary>
    public class CLUITable : IEnumerable<CLUITable.CLUITableItem>
    {
        /// <summary>
        /// Stores the rows of the table
        /// </summary>
        public List<CLUITableItem> Items { get; } = new List<CLUITableItem>();

        /// <summary>
        /// Stores the alignment info for columns
        /// </summary>
        public bool[] RightAlignColumns { get; set; } = Array.Empty<bool>();

        /// <summary>
        /// Gets the amount of space between each item in the table. Default is 1
        /// </summary>
        public int Spacing { get; set; } = 1;

        /// <summary>
        /// Adds a new row with the provided values
        /// </summary>
        /// <param name="values"></param>
        public void Add(params string[] values)
        {
            Items.Add(new CLUITableItem(values));
        }

        /// <summary>
        /// Adds the provided row to the table
        /// </summary>
        /// <param name="item"></param>
        public void Add(CLUITableItem item)
        {
            Items.Add(item);
        }

        /// <summary>
        /// Adds a separator. Semantic function identical to running Add without arguments
        /// </summary>
        public void Separator()
        {
            Add();
        }

        /// <summary>
        /// Renders the table's contents as a string, optionally with color encoding
        /// </summary>
        /// <returns></returns>
        private string RenderString()
        {
            StringBuilder output = new StringBuilder();
            int[] columnSpaces = GetReservedSpaces();

            foreach (CLUITableItem row in Items)
            {
                for (int column = 0; column < row.Values.Length; column++)
                {
                    bool   alignRight = false;
                    string value = row.Values[column];
                    int    space = columnSpaces[column] + value.Length - CLUI.EncodedStringLength(value);

                    if (column < RightAlignColumns.Length)
                        alignRight = RightAlignColumns[column];

                    if (alignRight)
                        output.Append(value.PadLeft(space));

                    else
                        output.Append(value.PadRight(space));

                    output.Append(CLUI.EncodeColor(ConsoleColor.Gray));
                    output.Append(new string(' ', Spacing));
                }

                output.AppendLine();
            }

            return output.ToString();
        }

        /// <summary>
        /// Renders the table's contents to the CLI
        /// </summary>
        public void Write()
        {
            CLUI.Write(RenderString());
        }

        /// <summary>
        /// Renders the table's contents to the CLI
        /// </summary>
        public void WriteLine()
        {
            CLUI.WriteLine(RenderString());
        }

        /// <summary>
        /// Gets the amount of space that must be reserved for padding each column of the table
        /// </summary>
        /// <returns></returns>
        private int[] GetReservedSpaces()
        {
            int maxColumns = Items.Max(i => i.Values.Length);
            int[] output = new int[maxColumns];

            for (int column = 0; column < maxColumns; column++)
            {
                foreach (CLUITableItem row in Items)
                {
                    if (column < row.Values.Length)
                        output[column] = Math.Max(output[column], CLUI.EncodedStringLength(row.Values[column]));
                }
            }

            return output;
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CLUITableItem> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <inheritdoc />
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Represents a row in a CLUITable
        /// </summary>
        public record CLUITableItem(string[] Values);
    }
}
