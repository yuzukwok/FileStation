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
    public class UploadProcess
    {
        public long SaveAs(string saveFilePath, HttpPostedFile file, out long fullLenth)
        {
            //本地文件长度
            long lStartPos = 0;
            //开始读取位置
            long startPosition = 0;
            //结束读取位置
            int endPosition = 0;
            fullLenth = 0;
            var contentRange = HttpContext.Current.Request.Headers["Content-Range"];
            //bytes 10000-19999/1157632           
            if (!string.IsNullOrEmpty(contentRange))
            {
                contentRange = contentRange.Replace("bytes", "").Trim();
                string length = contentRange.Substring(contentRange.IndexOf("/") + 1);
                fullLenth = long.Parse(length);
                contentRange = contentRange.Substring(0, contentRange.IndexOf("/"));

                string[] ranges = contentRange.Split('-');
                startPosition = int.Parse(ranges[0]);
                endPosition = int.Parse(ranges[1]);
            }
            System.IO.FileStream fs = null;

            try
            {
                if (System.IO.File.Exists(saveFilePath))
                {
                    fs = System.IO.File.OpenWrite(saveFilePath);
                    lStartPos = fs.Length;

                }
                else
                {
                    fs = new System.IO.FileStream(saveFilePath, System.IO.FileMode.Create);
                    lStartPos = 0;
                }
                if (lStartPos > endPosition)
                {
                    fs.Close();
                    //返回当前文件大小，通知浏览器从此位置开始上传
                    return lStartPos;
                }
                else if (lStartPos < startPosition)
                {
                    lStartPos = startPosition;
                }
                else if (lStartPos > startPosition && lStartPos < endPosition)
                {
                    lStartPos = startPosition;
                }
                fs.Seek(lStartPos, System.IO.SeekOrigin.Current);
                byte[] nbytes = new byte[1024];
                int nReadSize = 0;
                nReadSize = file.InputStream.Read(nbytes, 0, nbytes.Length);
                while (nReadSize > 0)
                {
                    fs.Write(nbytes, 0, nReadSize);
                    nReadSize = file.InputStream.Read(nbytes, 0, nReadSize);
                }

                fs.Close();
            }
            catch (Exception ex)
            {
                if (fs != null)
                {
                    fs.Close();
                }
            }

            return endPosition;
        }

    }
}
