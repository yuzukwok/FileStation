using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileStation.Util.Model
{
  public  class AppInfo
    {
      public virtual string Id { get; set; }
      public virtual string Name { get; set; }
      public virtual bool IsDeleted { get; set; }
    }
}
