using System;
using System.Collections.Generic;

using System.Text;

namespace FileStation.SDK
{
    public class UploadResult
    {
        public UploadState Result { get; set; }
        /// <summary>
        /// 多文件上传
        /// </summary>
        public string FileId { get; set; }
    }

    public enum UploadState
    {
        OK,
        Err_MustPostVerb,
        Err_InVaildPassKey,
        Err_DiskFull,
        Err_UploadFaild
    }

    public class FileUploaded 
    {
        public string FileId { get; set; }
        public string FileName { get; set; }
    }
}
