using Submerge.Engine.Model;
using SubMerge.Engine.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SubMerge.Engine.UnitTests
{
    public class ProcessServiceShould
    {
        private readonly IProcessService service;
        private readonly string rootFilePath = $"{Directory.GetCurrentDirectory()}/Data/Files";

        public ProcessServiceShould()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            service = new ProcessService();
        }

        [Theory]
        [InlineData("/Inputs/tr.srt", 517)]
        public async Task GetFirstEntries_should_return_success(string filePath, int ExpectedLineCount)
        {
            //Arrange
            var fullPath = rootFilePath + filePath;
            var lines = await File.ReadAllLinesAsync(fullPath, Constants.TrEncoding);

            //Act
            var entries = await service.GetFirstEntriesAsync(lines.ToList());

            //Assert
            Assert.Equal(ExpectedLineCount, entries.Count());
        }

        [Theory]
        [InlineData("/Inputs/tr.srt", "/Inputs/en.srt", 497)]
        public async Task FillSecondEntries_should_return_success(string file1, string file2, int ExpectedLineCount)
        {
            //Arrange
            var fullPathOfFile1 = rootFilePath + file1;
            var fullPathOfFile2 = rootFilePath + file2;
            var linesOfFile1 = await File.ReadAllLinesAsync(fullPathOfFile1, Constants.TrEncoding);
            var linesOfFile2 = await File.ReadAllLinesAsync(fullPathOfFile2, Constants.TrEncoding);
            var entries = await service.GetFirstEntriesAsync(linesOfFile1.ToList());

            //Act
            entries = await service.FillSecondEntriesAsync(linesOfFile2, entries.ToList());
            var countOfFullRecords = entries.Where(e => !String.IsNullOrWhiteSpace(e.Text1) && !String.IsNullOrWhiteSpace(e.Text2)).Count();

            //Assert
            Assert.Equal(ExpectedLineCount, countOfFullRecords);
        }

    }
}
