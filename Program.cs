using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

public static class FileReportGenerator
{
    public static IEnumerable<string> EnumerateFilesRecursively(string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException($"Directory not found: {path}");

        return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
    }

    public static string FormatByteSize(long byteSize)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB" };
        double size = byteSize;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{Math.Round(size, 2)} {units[unitIndex]}";
    }

    public static XDocument CreateReport(IEnumerable<string> files)
    {
        var reportData = files
            .Select(file => new FileInfo(file))
            .GroupBy(info => string.IsNullOrEmpty(info.Extension) ? "no extension" : info.Extension.ToLowerInvariant())
            .Select(group => new
            {
                Type = group.Key,
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

        try
        {
            var files = EnumerateFilesRecursively(args[0]);
            XDocument report = CreateReport(files);
            report.Save(args[1]);
            Console.WriteLine($"Report generated successfully at {args[1]}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
