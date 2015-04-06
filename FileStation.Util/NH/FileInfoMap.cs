using FileStation.Util.Model;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileStation.Util.NH
{
    public class FileInfoMap : ClassMap<FileInfo>
    {
        public FileInfoMap()
        {
            Id(p => p.Id);
            Map(p => p.Ext);
            Map(p => p.FileId);
            Map(p => p.FileName);
            Map(p => p.PassKey);
            Map(p => p.MIME);
            Map(p => p.UploadDateTime);
            Map(p => p.FileLength);
            Map(p => p.Op);
         
          
        }

    }
}
