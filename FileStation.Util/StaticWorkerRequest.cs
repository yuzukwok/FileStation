using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FileStation.Util
{
    public class StaticWorkerRequest : HttpWorkerRequest
    {
        readonly HttpWorkerRequest _request;
        private readonly byte[] _buffer;

        public StaticWorkerRequest(HttpWorkerRequest request, byte[] buffer)
        {
            _request = request;
            _buffer = buffer;
        }

        public override int ReadEntityBody(byte[] buffer, int size)
        {
            return 0;
        }

        public override int ReadEntityBody(byte[] buffer, int offset, int size)
        {
            return 0;
        }

        public override byte[] GetPreloadedEntityBody()
        {
            return _buffer;
        }

        public override int GetPreloadedEntityBody(byte[] buffer, int offset)
        {
            Buffer.BlockCopy(_buffer, 0, buffer, offset, _buffer.Length);
            return _buffer.Length;
        }

        public override int GetPreloadedEntityBodyLength()
        {
            return _buffer.Length;
        }

        public override int GetTotalEntityBodyLength()
        {
            return _buffer.Length;
        }

        public override string GetKnownRequestHeader(int index)
        {
            return index == HeaderContentLength
                       ? "0"
                       : _request.GetKnownRequestHeader(index);
        }

        // All other methods elided, they're just passthrough  

        public override void EndOfRequest()
        {
             _request.EndOfRequest();
        }

        public override void FlushResponse(bool finalFlush)
        {
            _request.FlushResponse(finalFlush);
        }

        public override string GetHttpVerbName()
        {
           return _request.GetHttpVerbName();
        }

        public override string GetHttpVersion()
        {
            return _request.GetHttpVersion();
        }

        public override string GetLocalAddress()
        {
            return _request.GetLocalAddress();
        }

        public override int GetLocalPort()
        {
            return _request.GetLocalPort();
        }

        public override string GetQueryString()
        {
            return _request.GetQueryString();
        }

        public override string GetRawUrl()
        {
            return _request.GetRawUrl();
        }

        public override string GetRemoteAddress()
        {
            return _request.GetRemoteAddress();
        }

        public override int GetRemotePort()
        {
            return _request.GetRemotePort();
        }

        public override string GetUriPath()
        {
            return _request.GetUriPath();
        }

        public override void SendKnownResponseHeader(int index, string value)
        {
            _request.SendKnownResponseHeader(index, value);
        }

        public override void SendResponseFromFile(IntPtr handle, long offset, long length)
        {
            _request.SendResponseFromFile(handle, offset, length);
        }

        public override void SendResponseFromFile(string filename, long offset, long length)
        {
            _request.SendResponseFromFile(filename, offset, length);
        }

        public override void SendResponseFromMemory(byte[] data, int length)
        {
            _request.SendResponseFromMemory(data, length);
        }

        public override void SendStatus(int statusCode, string statusDescription)
        {
            _request.SendStatus(statusCode, statusDescription);
        }

        public override void SendUnknownResponseHeader(string name, string value)
        {
            _request.SendUnknownResponseHeader(name, value);
        }
    }
}
