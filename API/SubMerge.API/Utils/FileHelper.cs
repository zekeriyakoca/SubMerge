using System.Text;

namespace SubMerge.API.Utils
{
    public static class FileHelper
    {
        public static IEnumerable<string> ReadLines(BinaryData binary)
        {
            return Engine.Utils.Constants.TrEncoding.GetString(binary.ToArray())
                .Split('\n');
        }
    }
}
