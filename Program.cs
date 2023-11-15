/*
Group Members:
Chase Calero, David Dang, Jonathan Kirtland, Eric Nguyen

Tools and material used:
https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/linq/introduction-to-linq-queries
https://www.c-sharpcorner.com/UploadFile/72d20e/concept-of-linq-with-C-Sharp/

Compilers:
Visual Studio 2022
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public static class FileReportGenerator
{
    // Enumerates all files in a given directory path, recursively including subdirectories.
    public static IEnumerable<string> EnumerateFilesRecursively(string path)
    {
        // Checks if the directory exists, throws an exception if not.
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        // Returns an enumerable collection of file names in the specified directory.
        return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
    }

    // Formats the size of bytes into a human-readable string with appropriate units.
    public static string FormatByteSize(long byteSize)
    {
        // Defines units for byte size representation.
        string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB" };
        double size = byteSize;
        int unitIndex = 0;

        // Converts the byte size to a higher unit if it exceeds 1024 of the current unit.
        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        // Returns the formatted byte size with two decimal places.
        return $"{Math.Round(size, 2)} {units[unitIndex]}";
    }

    // Creates an XML document report of file information based on a collection of file paths.
    public static XDocument CreateReport(IEnumerable<string> files)
    {
        // Groups files by their extension and aggregates count and size information.
        var reportData = files
            .Select(file => new FileInfo(file))
            // Check for extension
            .GroupBy(info => string.IsNullOrEmpty(info.Extension) ? "no extension" : info.Extension.ToLowerInvariant())
            .Select(group => new
            {
                Type = group.Key,
                Count = group.Count(),
                Size = group.Sum(info => info.Length)
            })
            .OrderByDescending(data => data.Size);

        // Constructs an HTML document with the file information.
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

        // Returns the constructed HTML document as an XDocument.
        return new XDocument(html);
    }

    public static void Main(string[] args)
    {
        // Checks for correct number of arguments, displays usage information if incorrect.
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: FileReportGenerator <input folder path> <output HTML report file path>");
            return;
        }

        try
        {
            // Generates file report based on input directory and saves the report to the specified path.
            var files = EnumerateFilesRecursively(args[0]);
            XDocument report = CreateReport(files);
            report.Save(args[1]);
            Console.WriteLine($"Report generated successfully at {args[1]}");
        }
        catch (Exception ex)
        {
            // Catches and displays any exceptions that occur during report generation.
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
