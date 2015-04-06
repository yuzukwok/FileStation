using FileStation.Util.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.InteropServices;
using System.Text;

namespace FileStation.Util.Manager
{
  public static  class FSCenter
    {

    public  static FileIdGenerator FileIdGenerator = new FileIdGenerator();
    public static string ErrMsg { get; set; }

    static  IISManager iiscontrol;
      public static string FirstFolder { get; set; }
      public static int FirstFolderNum { get; set; }
      public static string FirstFolderPath { get; set; }
      
      public  static string SecondFolder { get; set; }
      public static int SecondFolderNum { get; set; }
      public static string CurrentPath { get; set; }
      /// <summary>
      /// 单个文件夹最大文件数
      /// </summary>
      public static int FileCountMax { get; set; }
      /// <summary>
      /// 分区剩余最小空间，大小字节
      /// </summary>
      public static long MinFreeDiskSize { get; set; }

      public static ulong CurrentDiskMax { get;set;}
      public static ulong CurrentDisk { get; set; }
      public static int CurrentFileCount { get; set; }
      [DllImport("kernel32.dll")]
      public static extern bool GetDiskFreeSpaceEx(
              string lpDirectoryName,
              out UInt64 lpFreeBytesAvailable,
              out UInt64 lpTotalNumberOfBytes,
              out UInt64 lpTotalNumberOfFreeBytes);

      public static void Init() 
      {

          iiscontrol = new IISManager();
          iiscontrol.Connect();

          GetFirstFolder();

          GetSecondFolder();

          ReadConfig();


          ReadDiskInfo();

         
          if (MessageQueue.Exists(FileSyncMQ))
          {
              q = new MessageQueue(FileSyncMQ);
              

          }
          else
          {
              q = MessageQueue.Create(FileSyncMQ,true);
              q.SetPermissions("Everyone", System.Messaging.MessageQueueAccessRights.FullControl);
          }

      }
     static MessageQueue q;
      /// <summary>
      /// 验证passkey
      /// </summary>
      /// <param name="passkey"></param>
      /// <returns></returns>
      public static bool Vaild(string passkey) 
      {
          if (PassKeys==null)
          {
              PassKeys = new List<string>();
              //get
              UpdateAppKeyIds();
          }

          return PassKeys.Contains(passkey);

      }

      private static void UpdateAppKeyIds()
      {
          var session = NH.NHHelper.OpenSession();
          var list = session.QueryOver<AppInfo>().List<AppInfo>();

          if (list.Count > 0)
          {
              PassKeys = list.Select(p => p.Id).ToList();
          }
      }
      static IList<string> PassKeys;
      private static void ReadDiskInfo()
      {
          //获取磁盘信息
          var dirinfo = new DirectoryInfo(CurrentPath);
          CurrentFileCount = dirinfo.GetFiles().Length;
          //当前分区磁盘大小
          ulong freeBytesAvailable = 0;
          ulong totalNumberOfBytes = 0;
          ulong totalNumberOfFreeBytes = 0;
          GetDiskFreeSpaceEx(CurrentPath, out freeBytesAvailable, out totalNumberOfBytes, out totalNumberOfFreeBytes);
          CurrentDiskMax = totalNumberOfBytes;
          CurrentDisk = totalNumberOfBytes - totalNumberOfFreeBytes;
      }

      private static void ReadConfig()
      {
          //读取配置信息
          FileCountMax = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["FileCountMax"]);
          MinFreeDiskSize = long.Parse(System.Configuration.ConfigurationManager.AppSettings["MinFreeDiskSize"]);
      }

      private static void GetSecondFolder()
      {
          //第二级目录采用普通目录

          System.IO.DirectoryInfo info = new DirectoryInfo(FirstFolderPath);
          //读取所有二级目录名，取最大一个
          var directories=  info.GetDirectories();
          var flodermax = GetMaxNumByFolder(directories);
          if (flodermax == null)
          {
              //如果没有则创建虚拟目录 从00 开始
            
              SecondFolder = "00";
             
              CurrentPath = FirstFolderPath + @"\00";
              DirectoryInfo f = new DirectoryInfo(CurrentPath);
              if (!f.Exists)
              {
                  f.Create();
              }
             
             
          }
          else
          {
              //设置当前可用第一级目录名
              SecondFolder = flodermax;
              SecondFolderNum = Convert.ToInt32(SecondFolder, 16);
              CurrentPath = FirstFolderPath + "\\" + flodermax;
          }
      }

      private static string GetMaxNumByFolder(System.IO.DirectoryInfo[] directories)
      {
          IList<string> names = new List<string>();
          foreach (var item in directories)
          {

             
              if (item.Name.Length== 2)
              {
                  try
                  {
                      var s = Convert.ToInt32(item.Name, 16);
                      names.Add(item.Name);
                  }
                  catch (Exception)
                  {


                  }

              }
          }

          var name = names.OrderByDescending(p => p).FirstOrDefault();
          if (string.IsNullOrEmpty(name))
          {
              return null;
          }
          else
          {
              return name;
          }
      }

      private static void GetFirstFolder()
      {
          FirstFolderNum = 0;
          //检测文件存储目录

          // 读取IIS 虚拟目录（第一级文件夹）
          var iisfirstvr = iiscontrol.GetVirDirs();

          //取得数值最大的虚拟目录
          var iisvrmax = GetMaxNumByIISVir(iisfirstvr);
          if (iisvrmax == null)
          {
              //如果没有则创建虚拟目录 从00 开始
              VirtualDirectory newdir = new VirtualDirectory("00");
              FirstFolder = "00";
              //默认从D盘开始使用
              newdir.Path = @"D:\00";
              FirstFolderPath = newdir.Path;
              iiscontrol.Create(newdir);
          }
          else
          {
              //设置当前可用第一级目录名
              FirstFolder = iisvrmax.Name;
              FirstFolderNum = Convert.ToInt32(FirstFolder, 16);
              FirstFolderPath = iisvrmax.Path;
          }
      }

      private static VirtualDirectory GetMaxNumByIISVir(VirtualDirectories iisfirstvr)
      {
          IList<string> names=new List<string>();
          foreach (var item in iisfirstvr)
          {

              VirtualDirectory v = (VirtualDirectory)(((System.Collections.DictionaryEntry)item).Value);
              if (v.Name.Length == 2 )
              {
                  try
                  {
                     var s= Convert.ToInt32(v.Name, 16);
                     names.Add(v.Name);
                  }
                  catch (Exception)
                  {
                      
                      
                  }
                  
              }
          }

          var name = names.OrderByDescending(p => p).FirstOrDefault();
       if (string.IsNullOrEmpty(name))
       {
           return null;
       }
       else
       {
           return iisfirstvr.Find(name);
       }
          
      }
      /// <summary>
      /// 立刻切换一个目录，设置虚拟目录到新的分区上磁盘
      /// </summary>
      /// <returns></returns>
      public static void ChangeDisk()
      {
          
          //更换1级目录
          var str = String.Format("{0:X2}", FirstFolderNum + 1);
          VirtualDirectory n = new VirtualDirectory(str);
          //获取当前分区
          var fq = FirstFolderPath.Split(':')[0];
        var list=  DriveInfo.GetDrives();
        string next = "";
        foreach (var item in list)
        {
            if (item.DriveType!= DriveType.CDRom&&item.DriveType!= DriveType.Removable)
            {
                if (!System.Environment.SystemDirectory.Contains(item.Name))
                {
                    if (item.AvailableFreeSpace>MinFreeDiskSize&&!FirstFolderPath.Contains(item.Name))
                     {
                         next = item.Name;
                         break;
                    
                      } 
                }               
            }



        }

        if (string.IsNullOrEmpty(next))
        {
            ErrMsg = "服务器无可用磁盘，空间不足，请联系管理员";
            throw new Exception("服务器无可用磁盘，空间不足，请联系管理员");
        }
          n.Path = next + str;
          DirectoryInfo f = new DirectoryInfo(n.Path);
          if (!f.Exists)
          {
              f.Create();
          }
          iiscontrol.Create(n);

          FirstFolderNum++;
          FirstFolder = str;
          FirstFolderPath = n.Path;
          //创建2级
          CurrentFileCount = 0;
          SecondFolder = "00";
          SecondFolderNum = 0;
          CurrentPath = FirstFolderPath + @"\00";
          DirectoryInfo f2 = new DirectoryInfo(CurrentPath);
          if (!f2.Exists)
          {
              f2.Create();
          }
      }
      /// <summary>
      /// 更换文件夹
      /// </summary>
      public static void ChangeFolder()
      {
         //判断需要更换的是二级文件夹么
          if (SecondFolderNum<Convert.ToInt32("FF",16))
          {              
              //创建新的目录
              var str =  String.Format("{0:X2}", SecondFolderNum+1);
             
              string np =FirstFolderPath+"\\"+str;
              DirectoryInfo f = new DirectoryInfo(np);
              if (!f.Exists)
              {
                  f.Create();
              }
           
              //更新字段
            SecondFolderNum++;
            SecondFolder = str;
            CurrentPath =np;
            CurrentFileCount = 0;
             
          }
          else
          {
              //更换一级文件夹
              //创建虚拟目录
              var str = String.Format("{0:X2}", FirstFolderNum + 1);
              VirtualDirectory n=new VirtualDirectory(str);
              n.Path=FirstFolderPath.Split(':')[0]+":\\"+str;
              DirectoryInfo f = new DirectoryInfo(n.Path);
              if (!f.Exists)
              {
                  f.Create();
              }
              iiscontrol.Create(n);

              FirstFolderNum++;
              FirstFolder = str;
              FirstFolderPath = n.Path;
              //创建2级
              CurrentFileCount = 0;
              SecondFolder = "00";
              SecondFolderNum = 0;
              CurrentPath = FirstFolderPath + @"\00";
              DirectoryInfo f2 = new DirectoryInfo(CurrentPath);
              if (!f2.Exists)
              {
                  f2.Create();
              }
            
          }
      }


      public static string GetPhsyicPath(string floder1, string floder2)
      {
          var p = iiscontrol.GetVirDir(floder1, floder2);
          if (p!=null)
          {
              return p.Path;
          }
          else
          {
              return "";
          }
      }


      public static void SaveInfo(string fileid,string passkey,string fileName,string MIME,string Ext,int FileLength,string op)
      {
          var session = NH.NHHelper.OpenSession();
          using (session)
          {
              FileStation.Util.Model.FileInfo f = new Model.FileInfo();
              f.Op = op;
              f.PassKey = passkey;
              f.UploadDateTime = DateTime.Now;
              f.MIME = MIME;
              f.FileName = fileName;
              f.FileLength = FileLength;
              f.FileId = fileid;
              f.Ext = Ext;
              session.SaveOrUpdate(f);
              
          }
      }
      static string FileSyncMQ = @".\private$\filesync";//队列名称
      public static void SendMQInfo(string fileid, string oldfile, string op)
      {
          
         
          Message m = new Message();
          m.Label = fileid;      //描述消息的字串
          m.Body = op+","+oldfile;        //消息的主体
          q.Send(m,MessageQueueTransactionType.Single);      
      }
    }
}
