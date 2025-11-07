using System;
using System.IO;
using System.IO.Compression;

namespace GlobalDeclar
{
    public static class UniltHelp
    {

        #region 压缩/解压缩 数据

        //DeflateStream 方法
        /// <summary>压缩数据流</summary>
        /// <param name="inStream">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream Compress(this Stream inStream, Stream outStream = null)
        {
            var ms = outStream ?? new MemoryStream();

            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
            using (var stream = new DeflateStream(ms, CompressionLevel.Optimal, true))
            {
                inStream.CopyTo(stream);
                stream.Flush();
            }

            // 内部数据流需要把位置指向开头
            if (outStream == null) ms.Position = 0;

            return ms;
        }

        /// <summary>解压缩数据流</summary>
        /// <returns>Deflate算法，如果是ZLIB格式，则前面多两个字节，解压缩之前去掉，RocketMQ中有用到</returns>
        /// <param name="inStream">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream Decompress(this Stream inStream, Stream outStream = null)
        {
            var ms = outStream ?? new MemoryStream();

            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
            using (var stream = new DeflateStream(inStream, CompressionMode.Decompress, true))
            {
                stream.CopyTo(ms);
            }

            // 内部数据流需要把位置指向开头
            if (outStream == null) ms.Position = 0;

            return ms;
        }

        /// <summary>压缩字节数组</summary>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        public static Byte[] Compress(this Byte[] data)
        {
            var ms = new MemoryStream();
            Compress(new MemoryStream(data), ms);
            return ms.ToArray();
        }

        /// <summary>解压缩字节数组</summary>
        /// <returns>Deflate算法，如果是ZLIB格式，则前面多两个字节，解压缩之前去掉，RocketMQ中有用到</returns>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        public static Byte[] Decompress(this Byte[] data)
        {
            var ms = new MemoryStream();
            Decompress(new MemoryStream(data), ms);
            return ms.ToArray();
        }

        // gzip方法
        /// <summary>压缩数据流</summary>
        /// <param name="inStream">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream CompressGZip(this Stream inStream, Stream outStream = null)
        {
            var ms = outStream ?? new MemoryStream();

            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
            using (var stream = new GZipStream(ms, CompressionLevel.Optimal, true))
            {
                inStream.CopyTo(stream);
                stream.Flush();
            }

            // 内部数据流需要把位置指向开头
            if (outStream == null) ms.Position = 0;

            return ms;
        }

        /// <summary>解压缩数据流</summary>
        /// <param name="inStream">输入流</param>
        /// <param name="outStream">输出流。如果不指定，则内部实例化一个内存流</param>
        /// <remarks>返回输出流，注意此时指针位于末端</remarks>
        public static Stream DecompressGZip(this Stream inStream, Stream outStream = null)
        {
            var ms = outStream ?? new MemoryStream();

            // 第三个参数为true，保持数据流打开，内部不应该干涉外部，不要关闭外部的数据流
            using (var stream = new GZipStream(inStream, CompressionMode.Decompress, true))
            {
                stream.CopyTo(ms);
            }

            // 内部数据流需要把位置指向开头
            if (outStream == null) ms.Position = 0;

            return ms;
        }

        /// <summary>压缩字节数组</summary>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        public static Byte[] CompressGZip(this Byte[] data)
        {
            var ms = new MemoryStream();
            CompressGZip(new MemoryStream(data), ms);
            return ms.ToArray();
        }

        /// <summary>解压缩字节数组</summary>
        /// <returns>Deflate算法，如果是ZLIB格式，则前面多两个字节，解压缩之前去掉，RocketMQ中有用到</returns>
        /// <param name="data">字节数组</param>
        /// <returns></returns>
        public static Byte[] DecompressGZip(this Byte[] data)
        {
            var ms = new MemoryStream();
            DecompressGZip(new MemoryStream(data), ms);
            return ms.ToArray();
        }
        #endregion
    }
}
