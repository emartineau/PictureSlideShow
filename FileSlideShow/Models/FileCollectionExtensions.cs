using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSlideShow.Models
{
    public static class FileCollectionExtensions
    {
        public static IEnumerable<FileInfo> FilterFiles(
            this IEnumerable<FileInfo> fileInfos,
            ICollection<string> extensions) =>
            fileInfos.Where((f) => extensions.Contains(f.Extension.ToLower()));

        public static IEnumerable<FileInfo> Shuffle(this IEnumerable<FileInfo> fileInfos)
        {
            var ran = new Random();

            var fileArr = fileInfos.ToArray();
            for (int n = fileArr.Length - 1; n > 0; n--)
            {
                var ind = ran.Next(n);
                (fileArr[n], fileArr[ind]) = ((fileArr[ind]), fileArr[n]);
            }

            return fileArr;
        }
    }
}
