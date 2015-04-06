using FileStation.Util.Manager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace FileStation.Util
{
    public class UploadProcessor
    {
        private byte[] _buffer;
        private byte[] _boundaryBytes;
        private byte[] _endHeaderBytes;
        private byte[] _endFileHeaderBytes;
        private byte[] _endFileBytes;
        private byte[] _lineBreakBytes;
        private const string _lineBreak = "\r\n";
        private readonly Regex _filename = new Regex(@"Content-Disposition:\s*form-data\s*;\s*name\s*=\s*""file""\s*;\s*filename\s*=\s*""(.*)""",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private readonly HttpWorkerRequest _workerRequest;
        public UploadProcessor(HttpWorkerRequest workerRequest)
        {
            _workerRequest = workerRequest;
        }
        public void StreamToDisk(IServiceProvider provider, Encoding encoding, string filepath,ref string  orgfileName,ref string  ext,ref string  mime)
        {
            var buffer = new byte[8192];
            if (!_workerRequest.HasEntityBody())
            {
                return;
            }
            var total = _workerRequest.GetTotalEntityBodyLength();
            var preloaded = _workerRequest.GetPreloadedEntityBodyLength();
            var loaded = preloaded;
            SetByteMarkers(_workerRequest, encoding);
            var body = _workerRequest.GetPreloadedEntityBody();
            if (body == null) // IE normally does not preload
            {
                body = new byte[8192];
                preloaded = _workerRequest.ReadEntityBody(body, body.Length);
                loaded = preloaded;
            }
            var text = encoding.GetString(body);
            //获取文件名
            var fileName = _filename.Matches(text)[0].Groups[1].Value;
           // fileName = Path.GetFileName(fileName); // IE captures full user path; chop it
           // var path = Path.Combine(rootPath, fileName);
            orgfileName = fileName;
            //按.取扩展名
            var exts = fileName.Split('.');
            if (exts!=null&&exts.Length>=2)
            {
                ext ="."+ exts[exts.Length-1];
                filepath = filepath + ext;
                mime = MIME.GetMimeType(ext);
            }
            var files = new List<string>() { fileName };
            var stream = new FileStream(filepath, FileMode.Create);
            if (preloaded > 0)
            {
                stream = ProcessHeaders(body, stream, encoding, preloaded, files, filepath);
            }
            // Used to force further processing (i.e. redirects) to avoid buffering the files again
            var workerRequest = new StaticWorkerRequest(_workerRequest, body);
            var field = HttpContext.Current.Request.GetType().GetField("_wr", BindingFlags.NonPublic | BindingFlags.Instance); field.SetValue(HttpContext.Current.Request, workerRequest);
            if (!_workerRequest.IsEntireEntityBodyIsPreloaded())
            {
                var received = preloaded;
                while (total - received >= loaded && _workerRequest.IsClientConnected())
                {
                    loaded = _workerRequest.ReadEntityBody(buffer, buffer.Length);
                    stream = ProcessHeaders(buffer, stream, encoding, loaded, files, filepath);
                    received += loaded;
                }
                var remaining = total - received;
                buffer = new byte[remaining];
                loaded = _workerRequest.ReadEntityBody(buffer, remaining);
                stream = ProcessHeaders(buffer, stream, encoding, loaded, files, filepath);
            }
            stream.Flush();
            stream.Close();
            stream.Dispose();
        }
        private void SetByteMarkers(HttpWorkerRequest workerRequest, Encoding encoding)
        {
            var contentType = workerRequest.GetKnownRequestHeader(HttpWorkerRequest.HeaderContentType);
            var bufferIndex = contentType.IndexOf("boundary=") + "boundary=".Length;
            var boundary = String.Concat("--", contentType.Substring(bufferIndex));
            _boundaryBytes = encoding.GetBytes(string.Concat(boundary, _lineBreak));
            //head 结尾标志 (假定！hack，不同的表单或许会不一样的)
            _endFileHeaderBytes = encoding.GetBytes(string.Concat("application/octet-stream"+_lineBreak, _lineBreak));
            _endHeaderBytes = encoding.GetBytes(string.Concat(_lineBreak, _lineBreak));
            //文件结尾标志
           // _endFileBytes = encoding.GetBytes(string.Concat(_lineBreak, boundary, "--", _lineBreak));
           _endFileBytes = encoding.GetBytes(string.Concat(_lineBreak, boundary, "--"));
           
            _lineBreakBytes = encoding.GetBytes(string.Concat(_lineBreak + boundary + _lineBreak));
        }
        private FileStream ProcessHeaders(byte[] buffer, FileStream stream, Encoding encoding, int count, IList<string> files, string filepath)
        {
            //我们需要考虑buffer中会出现多个边界的
            buffer = AppendBuffer(buffer, count);
            //如果出现boundary 则代表需要特别处理这个buffer，去掉头，解析文件名等操作
            var startIndex = IndexOf(buffer, _boundaryBytes, 0);
            if (startIndex != -1)
            {
                //判断这个buffer中是否出现文件结束标志，出现则删除最末尾的_boundaryBytes，和出现的所有头
                var endFileIndex = IndexOf(buffer, _endFileBytes, 0);
                if (endFileIndex != -1)
                {
                   
                    //判断buffer中有file header end标志的话，删除之前的内容
                    var endHeaderIndex = IndexOf(buffer,_endFileHeaderBytes, 0);
                    if (endHeaderIndex != -1)
                    {
                        endHeaderIndex += _endFileHeaderBytes.Length;
                        buffer = SkipInput(buffer, startIndex, endHeaderIndex, ref count);
                    }
                   
                    //删除最后的边界,文件后面的边界内容全部剔除
                    var precedingBreakIndex = IndexOf(buffer, _boundaryBytes, 0);
                    if (precedingBreakIndex > -1)
                    {
                        startIndex = precedingBreakIndex; 
                        buffer = SkipInput(buffer, startIndex, buffer.Length, ref count);
                    }
                    //如果还存在结束标志的话，直接删除
                    var FileIndex = IndexOf(buffer, _endFileBytes, 0);
                    if (FileIndex!=-1)
                    {
                        buffer = SkipInput(buffer, FileIndex, buffer.Length, ref count);
                    }
                   

                   
                    stream.Write(buffer, 0, count);
                }
                else
                {
                    //没有出现文件尾,只要看有没有头，有则删除
                    var endHeaderIndex = IndexOf(buffer, _endFileHeaderBytes, 0);
                    if (endHeaderIndex != -1)
                    {
                       //出现文件头
                        //删除边界，只保留文件内容
                        endHeaderIndex += _endFileHeaderBytes.Length;
                        var modified = SkipInput(buffer, startIndex, endHeaderIndex, ref count);
                            stream.Write(modified, 0, count);

                    }
                    else
                    {
                        //没有出现任何头
                        _buffer = buffer;
                    }
                }
            }
            else
            {
                stream.Write(buffer, 0, count);
            }
            return stream;
        }
        private static FileStream ProcessNextFile(FileStream stream, byte[] buffer, int count, int startIndex, int endIndex, string filePath)
        {
            var fullCount = count;
            var endOfFile = SkipInput(buffer, startIndex, count, ref count);
            stream.Write(endOfFile, 0, count);
            stream.Flush();
            stream.Close();
            stream.Dispose();
            stream = new FileStream(filePath, FileMode.Create);
            var startOfFile = SkipInput(buffer, 0, endIndex, ref fullCount);
            stream.Write(startOfFile, 0, fullCount);
            return stream;
        }
        private static int IndexOf(byte[] array, IList value, int startIndex)
        {
            var index = 0;
            var start = Array.IndexOf(array, value[0], startIndex);
            if (start == -1)
            {
                return -1;
            }
            while ((start + index) < array.Length)
            {
                if (array[start + index] == (byte)value[index])
                {
                    index++;
                    if (index == value.Count)
                    {
                        return start;
                    }
                }
                else
                {
                    start = Array.IndexOf(array, value[0], start + index);
                    if (start != -1)
                    {
                        index = 0;
                    }
                    else
                    {
                        return -1;
                    }
                }
            }
            return -1;
        }
        private static byte[] SkipInput(byte[] input, int startIndex, int endIndex, ref int count)
        {
            var range = endIndex - startIndex;
            var size = count - range;
            var modified = new byte[size];
            var modifiedCount = 0;
            for (var i = 0; i < input.Length; i++)
            {
                if (i >= startIndex && i < endIndex)
                {
                    continue;
                }
                if (modifiedCount >= size)
                {
                    break;
                }
                modified[modifiedCount] = input[i];
                modifiedCount++;
            }
            input = modified;
            count = modified.Length;
            return input;
        }
        private byte[] AppendBuffer(byte[] buffer, int count)
        {
            var input = new byte[_buffer == null ? buffer.Length : _buffer.Length + count];
            if (_buffer != null)
            {
                Buffer.BlockCopy(_buffer, 0, input, 0, _buffer.Length);
            }
            Buffer.BlockCopy(buffer, 0, input, _buffer == null ? 0 : _buffer.Length, count);
            _buffer = null;
            return input;
        }
    }
}
