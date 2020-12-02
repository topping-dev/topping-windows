#if NETFX_CORE && ENABLE_STREAM_EXTENSIONS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace System.IO
{
    public static class StreamExtension
    {
        public static async Task<IRandomAccessStream> ToRandomAccessStream(this MemoryStream stream)
        {
            var randomAccessStream = new InMemoryRandomAccessStream();

            var outputStream = randomAccessStream.GetOutputStreamAt(0);
            var dw = new DataWriter(outputStream);
            var task = new Task(() => dw.WriteBytes(stream.ToArray()));
            task.Start();

            await task;
            await dw.StoreAsync();

            await outputStream.FlushAsync();

            return randomAccessStream;
        }

        public static async Task<IRandomAccessStream> ToRandomAccessStream(this IInputStream stream)
        {
            IRandomAccessStream inMemoryStream = new InMemoryRandomAccessStream();
            using (stream)
            {
                await RandomAccessStream.CopyAsync(stream, inMemoryStream);
            }
            inMemoryStream.Seek(0);
            return inMemoryStream;
        }

        public static async Task<IRandomAccessStream> AsRandomAccessStreamAsync(this Stream stream)
        {
            IRandomAccessStream inMemoryStream = new InMemoryRandomAccessStream();
            using (var inputStream = stream.AsInputStream())
            {
                await RandomAccessStream.CopyAsync(inputStream, inMemoryStream);
            }
            inMemoryStream.Seek(0);
            return inMemoryStream;
        }

        public static IRandomAccessStream AsRandomAccessStream(this Stream stream)
        {
            return new MemoryRandomAccessStream(stream);
        }
    }
}

#endif