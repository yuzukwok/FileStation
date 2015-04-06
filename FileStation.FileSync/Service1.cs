using FileStation.Util.FileSync;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace FileStation.FileSync
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }
        ReadFileInfoByMQ mq;
        private static ILog logger = LogManager.GetLogger("Service");
        protected override void OnStart(string[] args)
        {
           
            string assemblyFilePath = Assembly.GetExecutingAssembly().Location;
            string assemblyDirPath = Path.GetDirectoryName(assemblyFilePath);
            string configFilePath = assemblyDirPath + "\\log4netconfig.xml";
            log4net.Config.XmlConfigurator.Configure(new FileInfo(configFilePath));
            logger.Debug("开启FileSync服务");
            mq= new ReadFileInfoByMQ();
            mq.Start();
        }

        protected override void OnStop()
        {
            mq.Stop();
        }
    }
}
