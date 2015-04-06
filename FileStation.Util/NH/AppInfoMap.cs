using FileStation.Util.Model;
using FluentNHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileStation.Util.NH
{
    public class AppInfoMap : ClassMap<AppInfo>
    {
        public AppInfoMap()
        {
            Id(p => p.Id);
            Map(p => p.Name);
            Map(p => p.IsDeleted);
          
        }
    }
}
