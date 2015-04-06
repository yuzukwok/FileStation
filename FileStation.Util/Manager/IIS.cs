using System;
using System.Data;
using System.DirectoryServices;
using System.Collections;
using System.IO;

namespace FileStation.Util.Manager
{

    public enum IISVer 
    {
        V5,
        V6,
        V7
    }

    /**/
    /// <summary>
    /// CreateWebDir 的摘要说明。
    /// </summary>
    public class IISManager
    {
        //定义需要使用的
        private string _server, _website, _AnonymousUserPass, _AnonymousUserName;
        private VirtualDirectories _virdirs;
        protected System.DirectoryServices.DirectoryEntry rootfolder;
        private bool _batchflag;
        public IISManager()
        {
            //默认情况下使用localhost，即访问本地机
            _server = "localhost";
            _website = "1";
            _batchflag = false;
            IISVer = GetIssVersionByDri(_server);
        }
        public IISManager(string strServer)
        {
            _server = strServer;
            _website = "1";
            _batchflag = false;
            IISVer = GetIssVersionByDri(strServer);
        }

        public IISVer IISVer { get; set; }

  
        
        public string AnonymousUserName
        {
            get { return _AnonymousUserName; }
            set { _AnonymousUserName = value; }
        }
        public string AnonymousUserPass
        {
            get { return _AnonymousUserPass; }
            set { _AnonymousUserPass = value; }
        }
        //Server属性定义访问机器的名字，可以是IP与计算名
        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }
        //WebSite属性定义，为一数字，为方便，使用string 
        //一般来说第一台主机为1,第二台主机为2，依次类推
        public string WebSite
        {
            get { return _website; }
            set { _website = value; }
        }

        //虚拟目录的名字
        public VirtualDirectories VirDirs
        {
            get { return _virdirs; }
            set { _virdirs = value; }
        }
        /**/
        ///<summary>
        ///定义公共方法
        ///</summary>
        public  IISVer GetIssVersionByDri(string server)
        {
            try
            {
                if (string.IsNullOrEmpty(server))
                {
                    //如果为空 则默认为本地机器 
                    server = "LOCALHOST";
                }
                DirectoryEntry getEntity = new DirectoryEntry("IIS://" + server + "/W3SVC/INFO");
                string Version = getEntity.Properties["MajorIISVersionNumber"].Value.ToString();
                if (Version.Contains("7"))
                {
                    return IISVer.V7;
                }
                else
                {
                    return IISVer.V6;
                }
            }
            catch (Exception se)
            {
                return IISVer.V5;
                //说明一点:IIS5.0中没有 (int)entry.Properties["MajorIISVersionNumber"].Value;属性，将抛出异常 证明版本为 5.0   

            }
        }
        //连接服务器
        public void Connect()
        {
            ConnectToServer();
        }
       
        
        //判断是否存这个虚拟目录
        public bool Exists(string strVirdir)
        {
            return _virdirs.Contains(strVirdir);
        }
        //添加一个虚拟目录
        public bool Create(VirtualDirectory newdir)
        {
            string strPath = "IIS://" + _server + "/W3SVC/" + _website + "/ROOT/" + newdir.Name;
            if (!_virdirs.Contains(newdir.Name) || _batchflag)
            {
                System.IO.DirectoryInfo info = new DirectoryInfo(newdir.Path);
                if (!info.Exists)
                {
                    info.Create();
                }

                try
                {
                    //加入到ROOT的Children集合中去
                    DirectoryEntry newVirDir = rootfolder.Children.Add(newdir.Name, "IIsWebVirtualDir");
                    newVirDir.Invoke("AppCreate", true);
                    newVirDir.CommitChanges();
                    rootfolder.CommitChanges();
                    //然后更新数据
                    UpdateDirInfo(newVirDir, newdir);
                    return true;
                }
                catch
                {
                    //throw new Exception(ee.ToString());
                    return false;
                }
            }
            else
            {
                return true;
                //throw new Exception("This virtual directory is already exist.");
            }
        }

        public bool Create(string  root,VirtualDirectory newdir)
        {
            string strPath = "IIS://" + _server + "/W3SVC/" + _website + "/ROOT/" + newdir.Name;
            var dirs = GetVirDirs(root);
            if (!dirs.Contains(newdir.Name) || _batchflag)
            {
                System.IO.DirectoryInfo info = new DirectoryInfo(newdir.Path);
                if (!info.Exists)
                {
                    info.Create();
                }

                try
                {
                    DirectoryEntry rootf = new DirectoryEntry("IIS://" + _server + "/W3SVC/" + _website + "/ROOT/" + root);
                    //加入到上一级的Children集合中去
                    DirectoryEntry newVirDir = rootf.Children.Add(newdir.Name, "IIsWebVirtualDir");
                    newVirDir.Invoke("AppCreate", true);
                    newVirDir.CommitChanges();
                    rootf.CommitChanges();
                    //然后更新数据
                    UpdateDirInfo(newVirDir, newdir);
                    return true;
                }
                catch
                {
                    //throw new Exception(ee.ToString());
                    return false;
                }
            }
            else
            {
                return true;
                //throw new Exception("This virtual directory is already exist.");
            }
        }
        //得到一个虚拟目录
        public VirtualDirectory GetVirDir(string strVirdir)
        {
            VirtualDirectory tmp = null;
            if (_virdirs.Contains(strVirdir))
            {
                tmp = _virdirs.Find(strVirdir);
                ((VirtualDirectory)_virdirs[strVirdir]).flag = 2;
            }
            else
            {
                //throw new Exception("This virtual directory is not exists");
            }
            return tmp;
        }

        public VirtualDirectory GetVirDir(string root,string strVirdir)
        {
            VirtualDirectory tmp = null;
            var dirs = GetVirDirs(root);
            if (dirs.Contains(strVirdir))
            {
                tmp = dirs.Find(strVirdir);
                ((VirtualDirectory)dirs[strVirdir]).flag = 2;
            }
            else
            {
                //throw new Exception("This virtual directory is not exists");
            }
            return tmp;
        }
       

        //更新一个虚拟目录
        public void Update(VirtualDirectory dir)
        {
            //判断需要更改的虚拟目录是否存在
            if (_virdirs.Contains(dir.Name))
            {
                DirectoryEntry ode = rootfolder.Children.Find(dir.Name, "IIsWebVirtualDir");
                UpdateDirInfo(ode, dir);
            }
            else
            {
                //throw new Exception("This virtual directory is not exists.");
            }
        }

        public void Update(string root,VirtualDirectory dir)
        {
            var dirs = GetVirDirs(root);
           var rootf = new DirectoryEntry("IIS://" + _server + "/W3SVC/" + _website + "/ROOT/" + root);
            //判断需要更改的虚拟目录是否存在
            if (dirs.Contains(dir.Name))
            {
                DirectoryEntry ode = rootf.Children.Find(dir.Name, "IIsWebVirtualDir");
                UpdateDirInfo(ode, dir);
            }
            else
            {
                //throw new Exception("This virtual directory is not exists.");
            }
        }

        //删除一个虚拟目录
        public void Delete(string strVirdir)
        {
            if (_virdirs.Contains(strVirdir))
            {
                object[] paras = new object[2];
                paras[0] = "IIsWebVirtualDir"; //表示操作的是虚拟目录
                paras[1] = strVirdir;
                rootfolder.Invoke("Delete", paras);
                rootfolder.CommitChanges();
            }
            else
            {
                //throw new Exception("Can''t delete " + strVirdir + ",because it isn''t exists.");
            }
        }
        //批量更新
        public void UpdateBatch()
        {
            BatchUpdate(_virdirs);
        }
        //重载一个：-)
        public void UpdateBatch(VirtualDirectories vds)
        {
            BatchUpdate(vds);
        }

        /**/
        ///<summary>
        ///私有方法
        ///</summary>

        public void Close()
        {
            _virdirs.Clear();
            _virdirs = null;
            rootfolder.Dispose();

        }
        //连接服务器
        private void ConnectToServer()
        {
            string strPath = "IIS://" + _server + "/W3SVC/" + _website + "/ROOT";
            try
            {
                this.rootfolder = new DirectoryEntry(strPath);
                _virdirs = GetVirDirs(this.rootfolder.Children);
            }
            catch (Exception e)
            {
                throw new Exception("Can''t connect to the server [" + _server + "] ", e);
            }
        }
        //执行批量更新
        private void BatchUpdate(VirtualDirectories vds)
        {
            _batchflag = true;
            foreach (object item in vds.Values)
            {
                VirtualDirectory vd = (VirtualDirectory)item;
                switch (vd.flag)
                {
                    case 0:
                        break;
                    case 1:
                        Create(vd);
                        break;
                    case 2:
                        Update(vd);
                        break;
                }
            }
            _batchflag = false;
        }
        //更新
        private void UpdateDirInfo(DirectoryEntry de, VirtualDirectory vd)
        {
            de.Properties["AnonymousUserName"][0] = vd.AnonymousUserName;
            de.Properties["AnonymousUserPass"][0] = vd.AnonymousUserPass;
            de.Properties["AccessRead"][0] = vd.AccessRead;
            de.Properties["AccessExecute"][0] = vd.AccessExecute;
            de.Properties["AccessWrite"][0] = vd.AccessWrite;
            de.Properties["AuthBasic"][0] = vd.AuthBasic;
            de.Properties["AuthNTLM"][0] = vd.AuthNTLM;
            de.Properties["ContentIndexed"][0] = vd.ContentIndexed;
            de.Properties["EnableDefaultDoc"][0] = vd.EnableDefaultDoc;
            de.Properties["EnableDirBrowsing"][0] = vd.EnableDirBrowsing;
            de.Properties["AccessSSL"][0] = vd.AccessSSL;
            de.Properties["AccessScript"][0] = vd.AccessScript;
            de.Properties["DefaultDoc"][0] = vd.DefaultDoc;
            de.Properties["Path"][0] = vd.Path;
            de.CommitChanges();
        }

        //获取虚拟目录集合
        public VirtualDirectories GetVirDirs(DirectoryEntries des)
        {
            VirtualDirectories tmpdirs = new VirtualDirectories();
            foreach (DirectoryEntry de in des)
            {
                if (de.SchemaClassName == "IIsWebVirtualDir")
                {
                    VirtualDirectory vd = new VirtualDirectory();
                    vd.Name = de.Name;
                    vd.AccessRead = (bool)de.Properties["AccessRead"][0];
                    vd.AccessExecute = (bool)de.Properties["AccessExecute"][0];
                    vd.AccessWrite = (bool)de.Properties["AccessWrite"][0];
                    vd.AnonymousUserName = (string)de.Properties["AnonymousUserName"][0];
                    vd.AnonymousUserPass = (string)de.Properties["AnonymousUserPass"][0];
                    vd.AuthBasic = (bool)de.Properties["AuthBasic"][0];
                    vd.AuthNTLM = (bool)de.Properties["AuthNTLM"][0];
                    vd.ContentIndexed = (bool)de.Properties["ContentIndexed"][0];
                    vd.EnableDefaultDoc = (bool)de.Properties["EnableDefaultDoc"][0];
                    vd.EnableDirBrowsing = (bool)de.Properties["EnableDirBrowsing"][0];
                    vd.AccessSSL = (bool)de.Properties["AccessSSL"][0];
                    vd.AccessScript = (bool)de.Properties["AccessScript"][0];
                    vd.Path = (string)de.Properties["Path"][0];
                    vd.flag = 0;
                    vd.DefaultDoc = (string)de.Properties["DefaultDoc"][0];
                    tmpdirs.Add(vd.Name, vd);
                }
            }
            return tmpdirs;
        }

        public VirtualDirectories GetVirDirs() 
        {
            return _virdirs;
        }
        public VirtualDirectories GetVirDirs(string root) 
        {
            var rootf = new DirectoryEntry("IIS://" + _server + "/W3SVC/" + _website + "/ROOT/" + root);
            return GetVirDirs(rootf.Children);
        }



    }
    /**/
    /// <summary>
    /// VirtualDirectory类
    /// </summary>
    public class VirtualDirectory
    {
        private bool _read, _execute, _script, _ssl, _write, _authbasic, _authntlm, _indexed, _endirbrow, _endefaultdoc;
        private string _ausername, _auserpass, _name, _path;
        private int _flag;
        private string _defaultdoc;
        /**/
        /// <summary>
        /// 构造函数
        /// </summary>
        public VirtualDirectory()
        {
            SetValue();
        }
        public VirtualDirectory(string sVirDirName)
        {
            SetValue();
            _name = sVirDirName;
        }
        // sVirDirName：虚拟路径; 
        // strPhyPath： 物理路径( physics Path)
        public VirtualDirectory(string sVirDirName, string strPhyPath, string[] AnonymousUser)
        {
            SetValue();
            _name = sVirDirName;
            _path = strPhyPath;
            _ausername = AnonymousUser[0];
            _auserpass = AnonymousUser[1];
        }
        private void SetValue()
        {
            _read = true; _execute = false; _script = true; _ssl = false; _write = false; _authbasic = false;
            _authntlm = true; _indexed = true; _endirbrow = false; _endefaultdoc = true;
            _flag = 1;
            _defaultdoc = "default.htm,default.aspx,default.asp,index.htm";
            _path = "C:\\";
            _ausername = "IUSR"; _auserpass = "IUSR"; _name = "";
        }
        /**/
        ///<summary>
        ///定义属性,IISVirtualDir太多属性了
        ///
        ///</summary>

        public int flag
        {
            get { return _flag; }
            set { _flag = value; }
        }
        public bool AccessRead
        {
            get { return _read; }
            set { _read = value; }
        }
        public bool AccessWrite
        {
            get { return _write; }
            set { _write = value; }
        }
        public bool AccessExecute
        {
            get { return _execute; }
            set { _execute = value; }
        }
        public bool AccessSSL
        {
            get { return _ssl; }
            set { _ssl = value; }
        }
        public bool AccessScript
        {
            get { return _script; }
            set { _script = value; }
        }
        public bool AuthBasic
        {
            get { return _authbasic; }
            set { _authbasic = value; }
        }
        public bool AuthNTLM
        {
            get { return _authntlm; }
            set { _authntlm = value; }
        }
        public bool ContentIndexed
        {
            get { return _indexed; }
            set { _indexed = value; }
        }
        public bool EnableDirBrowsing
        {
            get { return _endirbrow; }
            set { _endirbrow = value; }
        }
        public bool EnableDefaultDoc
        {
            get { return _endefaultdoc; }
            set { _endefaultdoc = value; }
        }
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
        public string DefaultDoc
        {
            get { return _defaultdoc; }
            set { _defaultdoc = value; }
        }
        public string AnonymousUserName
        {
            get { return _ausername; }
            set { _ausername = value; }
        }
        public string AnonymousUserPass
        {
            get { return _auserpass; }
            set { _auserpass = value; }
        }
    }
    /**/
    /// <summary>
    /// 集合VirtualDirectories
    /// </summary>

    public class VirtualDirectories : System.Collections.Hashtable
    {
        public VirtualDirectories()
        {
        }
        //添加新的方法 
        public VirtualDirectory Find(string strName)
        {
            return (VirtualDirectory)this[strName];
        }
    }



    public class DirectoryInfos : System.Collections.CollectionBase
    {
        public int Add(DirectoryInfo di)
        {
            return this.List.Add(di);
        }

        public DirectoryInfo this[int index]
        {
            get { return (DirectoryInfo)List[index]; }
        }

        public void AddCollection(DirectoryInfo[] dirs)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                List.Add(dir);
            }
        }
    }

    public enum LogState
    {
        Success,
        Failed,
        Message
    }

}
