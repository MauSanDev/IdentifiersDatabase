using System.IO;
using System.Collections.Generic;

public static class CSVGenerator
{
    public static void GenerateCSVFile(string directoryPath, string fileName, ICSVDataHandler csvDataHandler)
    {
        if(!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        fileName = fileName.Replace(" ", string.Empty);
        string finalPath = Path.Combine(directoryPath, fileName);
        
        StreamWriter streamWriter = new StreamWriter(finalPath);
        streamWriter.Write(csvDataHandler.CSVData);
        streamWriter.Flush();
        streamWriter.Close();
    }

    public static string GetCSVLineForElements(params string[] elements) => string.Join(";", elements) + ";\n";
    public static string GetCSVLineForElements(ICSVDataHandler csvDataHandler, params string[] elements) => csvDataHandler.CSVData.Replace("\n", "") + string.Join(";", elements) + ";\n";

    public static string MergeCSVHandlers<T>(List<T> csvHandlers) where T : ICSVDataHandler
    {
        string toReturn = string.Empty;
        for (int i = 0; i < csvHandlers.Count; i++)
        {
            toReturn += csvHandlers[i].CSVData;
        }

        return toReturn;
    }
}
