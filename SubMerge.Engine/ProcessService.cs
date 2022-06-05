using System.Text;
using Submerge.Engine.Model;
using SubMerge.Engine.Utils;

namespace SubMerge.Engine
{
    public interface IProcessService
    {
        Task<IEnumerable<Entry>> FillSecondEntries(IEnumerable<string> lines, List<Entry> entries);
        Task<IEnumerable<Entry>> GetFirstEntries(IEnumerable<string> lines);
        IEnumerable<Entry> TryFixEntries(List<Entry> result);
    }

    public class ProcessService : IProcessService
    {
        public Encoding TrEncoding => Constants.TrEncoding;
        public string DateSeperator => "-->";
        public string TimeFormat => @"hh\:mm\:ss\,fff";

        public ProcessService()
        {
        }

        public IEnumerable<Entry> TryFixEntries(List<Entry> result)
        {
            for (int i = 1; i < result.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(result[i].Text2))
                {
                    if (!String.IsNullOrWhiteSpace(result[i - 1].Text2))
                    {
                        result[i - 1] = result[i - 1] with { Text1 = $"{ result[i - 1].Text1 }{ result[i].Text1 }" };
                    }
                }
            }
            return result.Where(r => !String.IsNullOrWhiteSpace(r.Text1) && !String.IsNullOrWhiteSpace(r.Text2)).ToList();
        }

        public async Task<IEnumerable<Entry>> GetFirstEntries(IEnumerable<string> lines)
        {
            Entry entry;
            TimeSpan timeStart = default, timeEnd = default;
            List<Entry> entries = new List<Entry>();
            var enumerator = lines
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .SkipWhile(l => l.Contains(DateSeperator))
                .GetEnumerator();

            enumerator.MoveNext();
            int recordCounter = 1;

            StringBuilder text = new();

            do
            {
                if (enumerator.Current.Contains(DateSeperator))
                {
                    var isFirstRecord = timeStart == default;

                    if (!isFirstRecord)
                    {
                        entries.Add(BuildEntry(timeStart, timeEnd, text, String.Empty));
                        text.Clear();
                    }

                    var spans = enumerator.Current.Split(DateSeperator);
                    TimeSpan.TryParseExact(spans.First().Trim(), TimeFormat, null, out timeStart);
                    TimeSpan.TryParseExact(spans.Last().Trim(), TimeFormat, null, out timeEnd);
                }
                else
                {
                    if (enumerator.Current.Trim() == recordCounter.ToString())
                    {
                        recordCounter++;
                        continue;
                    }
                    text.AppendLine(enumerator.Current);
                }
            } while (enumerator.MoveNext());
            return entries;
        }

        public async Task<IEnumerable<Entry>> FillSecondEntries(IEnumerable<string> lines, List<Entry> entries)
        {
            Entry entry;
            TimeSpan timeStart = default, timeEnd = default;
            var fileEnumerator = lines
                .Where(l => !String.IsNullOrWhiteSpace(l))
                .SkipWhile(l => l.Contains(DateSeperator))
                .GetEnumerator();

            fileEnumerator.MoveNext();
            int recordCounter = 1;
            var entryEnumerator = entries.GetEnumerator();

            StringBuilder text = new();
            int i = 0;
            do
            {
                if (fileEnumerator.Current.Contains(DateSeperator))
                {
                    var isFirstRecord = timeStart == default;
                    if (!isFirstRecord)
                    {
                        for (; i < entries.Count(); i++)
                        {
                            var current = entries.ElementAt(i);

                            if (current == null)
                                break;
                            if (timeEnd < current.Start)
                            {
                                text.Clear();
                                break;
                            }
                            else if ((current.Start <= timeStart && timeStart <= current.End) ||
                                (timeStart <= current.Start && current.Start <= timeEnd))
                            {
                                entries[i] = current with { Text2 = text.ToString() };
                                text.Clear();
                                i++;
                                break;
                            }
                        }

                    }

                    var spans = fileEnumerator.Current.Split(DateSeperator);
                    TimeSpan.TryParseExact(spans.First().Trim(), TimeFormat, null, out timeStart);
                    TimeSpan.TryParseExact(spans.Last().Trim(), TimeFormat, null, out timeEnd);
                }
                else
                {
                    if (fileEnumerator.Current.Trim() == recordCounter.ToString())
                    {
                        recordCounter++;
                        continue;
                    }
                    text.AppendLine(fileEnumerator.Current);
                }
            } while (fileEnumerator.MoveNext());

            return entries;
        }

        private Entry BuildEntry(TimeSpan timeStart, TimeSpan timeEnd, StringBuilder text, string empty)
        {
            return new Entry(timeStart, timeEnd, text.ToString(), String.Empty);
        }
    }
}
