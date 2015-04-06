using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileStation.Util.Model
{
   public class FileInfo
    {
       public virtual int Id { get; set; }
       public virtual string PassKey { get; set; }
       public virtual string FileId { get; set; }
       public virtual string FileName { get; set; }
       public virtual string Ext { get; set; }
       public virtual string MIME { get; set; }
       public virtual DateTime UploadDateTime { get; set; }
       public virtual int FileLength { get; set; }
       public virtual string Op { get; set; }
    }
}
