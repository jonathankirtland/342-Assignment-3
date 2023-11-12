// File: FileReportGenerator.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public static class FileReportGenerator
{
    // Enumerates all files in a given folder recursively, including subfolders.
    public static IEnumerable<string> EnumerateFilesRecursively(string path)
    {
        if (!Directory.Exists(path))
        {
            throw new DirectoryNotFoundException($"Directory not found: {path}");
        }

        return EnumerateFiles(path);
    }

    private static IEnumerable<string> EnumerateFiles(string path)
    {
        IEnumerable<string> files = null;

        try
        {
            files = Directory.EnumerateFiles(path);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error accessing files in: {path}. Error: {ex.Message}");
            yield break;
        }

        foreach (string file in files)
        {
            yield return file;
        }

        IEnumerable<string> directories = null;
        try
        {
            directories = Directory.EnumerateDirectories(path);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error accessing directories in: {path}. Error: {ex.Message}");
            yield break;
        }

        foreach (string directory in directories)
        {
            foreach (string file in EnumerateFilesRecursively(directory))
            {
                yield return file;
            }
        }
    }

    // Formats a byte size into a human-readable string with two decimal places.
    public static string FormatByteSize(long byteSize)
    {
        string[] units = { "B", "kB", "MB", "GB", "TB", "PB", "EB", "ZB" };
        double size = byteSize;
        int unitIndex = 0;

        while (size >= 1000 && unitIndex < units.Length - 1)
        {
            size /= 1000;
            unitIndex++;
        }

        return $"{Math.Round(size, 2)} {units[unitIndex]}";
    }

    // Creates an HTML report from a given collection of file paths.
    public static XDocument CreateReport(IEnumerable<string> files)
    {
        var reportData = files
            .Select(file => new FileInfo(file))
            .GroupBy(info => info.Extension.ToLowerInvariant())
            .Select(group => new
            {
                Type = string.IsNullOrEmpty(group.Key) ? "no extension" : group.Key,
                Count = group.Count(),
                Size = group.Sum(info => info.Length)
            })
            .OrderByDescending(data => data.Size);

        var html = new XElement("html",
            new XElement("body",
                new XElement("table",
                    new XElement("thead",
                        new XElement("tr",
                            new XElement("th", "Type"),
                            new XElement("th", "Count"),
                            new XElement("th", "Size"))),
                    new XElement("tbody",
                        reportData.Select(data =>
                            new XElement("tr",
                                new XElement("td", data.Type),
                                new XElement("td", data.Count),
                                new XElement("td", FormatByteSize(data.Size))))))));

        return new XDocument(html);
    }

    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: FileReportGenerator <input folder path> <output HTML report file path>");
            return;
        }

        string inputFolderPath = args[0];
        string outputReportPath = args[1];

        try
        {
            var files = EnumerateFilesRecursively(inputFolderPath);
            XDocument report = CreateReport(files);
            report.Save(outputReportPath);
            Console.WriteLine($"Report generated successfully at {outputReportPath}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
