
using Submerge.Engine.Model;
using SubMerge.Engine;
using SubMerge.Engine.Utils;
using System.Text;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

IProcessService service = new ProcessService();

var result = await GetEntries();

await RecordToFile(result, true);

Console.WriteLine("Finnish!");

async Task<IEnumerable<Entry>> GetEntries()
{
    var files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Files");
    var file1 = files[0];
    var file2 = files[1];

    List<Entry> entries = new();

    entries = (await service.GetFirstEntriesAsync(await ReadLines(file1))).ToList();

    entries = (await service.FillSecondEntriesAsync(await ReadLines(file2), entries)).ToList();

    entries = service.TryFixEntries(entries).ToList();

    return entries;
}

async Task RecordToFile(IEnumerable<Entry> entries, bool reverse = false)
{
    var outpubFilePath = Directory.GetCurrentDirectory() + "/Files/Output/result.txt";
    File.WriteAllLines(outpubFilePath, entries.Select(e => BuildRecord(e, reverse)));
}

string BuildRecord(Entry e, bool reverse)
{
    StringBuilder text = new StringBuilder();
    text.AppendLine(reverse ? e.Text2.Trim() : e.Text1.Trim());
    text.AppendLine("---------------");
    text.AppendLine(reverse ? e.Text1.Trim() : e.Text2.Trim());
    text.AppendLine(String.Empty);
    text.AppendLine("///////////////");
    return text.ToString();
}

async Task<IEnumerable<string>> ReadLines(string filePath)
{
    return (await File.ReadAllLinesAsync(filePath, Constants.TrEncoding)).ToList();
}