using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

public static class ASCIIMapTools
{

    public static string[][] DetextifyASCIIMap(string ASCIIMapString)
    {
        if (!IsAscii(ASCIIMapString))
            throw new ArgumentException("Input string contains non-ASCII characters.");

        string[] rows = ASCIIMapString.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
        int numRows = rows.Length;
        int numCols = rows[0].Length;
        string[][] ASCIIMapArray = new string[numRows][];

        for (int i = 0; i < numRows; i++)
        {
            if (rows[i].Length != numCols)
                throw new ArgumentException("Rows of map-string must be the same length.");
            ASCIIMapArray[i] = rows[i].Select(c => c.ToString()).ToArray();
        }

        return ASCIIMapArray;

    }

    public static string TextifyASCIIMapArray(string[][] ASCIIMapArray)
    {
        StringBuilder ASCIIMapStringBuilder = new StringBuilder();
        int numRows = ASCIIMapArray.Length;
        int numCols = ASCIIMapArray[0].Length;
        for (int i = 0; i < numRows; i++)
        {
            if (ASCIIMapArray[i].Length != numCols)
                throw new ArgumentException("Rows of map-string must be the same length.");
            ASCIIMapStringBuilder.Append(string.Join("",ASCIIMapArray[i]));
            if (i < numRows - 1) ASCIIMapStringBuilder.Append("\n");
        }

        string ASCIIMapString = ASCIIMapStringBuilder.ToString();
        if (!IsAscii(ASCIIMapString))
                throw new ArgumentException("Input Array contains non-ASCII characters.");
        return ASCIIMapString;
    }

    private static bool IsAscii(string input) {
        foreach (char c in input)
            if (c >= 128) return false;
        return true;
    }
}