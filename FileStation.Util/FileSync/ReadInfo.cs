using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;
using System.Text;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
using log4net;

namespace FileStation.Util.FileSync
{
  public  class ReadFileInfoByMQ
    {
      static MessageQueue q;
      static string FileSyncMQ;//队列名称
      static int MAX_WORKER_THREADS { get; set; }
      private IList<WaitHandle> waitHandleArray = new List<WaitHandle>();
      private static ILog logger = LogManager.GetLogger("ReadFileInfoByMQ");
      public void Start() 
      {
          //读取远程队列名称
          FileSyncMQ = ConfigurationManager.AppSettings["FileSyncMQ"];
          logger.Debug("远程队列名 " + FileSyncMQ);
          MAX_WORKER_THREADS = Convert.ToInt32(ConfigurationManager.AppSettings["MAX_WORKER_THREADS"].ToString());
          logger.Debug("异步读取工作线程数 " + MAX_WORKER_THREADS);
          try
          {
         
              
           q = new MessageQueue(FileSyncMQ);


          q.ReceiveCompleted += q_ReceiveCompleted;
            
            //异步方式，并发
            
            for(int i=0; i<MAX_WORKER_THREADS; i++)
            {
               
                waitHandleArray.Add( q.BeginReceive(MessageQueue.InfiniteTimeout).AsyncWaitHandle);
            }
               }
              catch (Exception ex)
              {
                  
                  logger.Error(ex);
              }
      }

      void q_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
      {
         //处理文件
        var  mq = (MessageQueue)sender;
         Message m = mq.EndReceive(e.AsyncResult);        
         string fileId = m.Label;
         logger.Debug(fileId);
          
          //再次接收
         mq.BeginReceive(MessageQueue.InfiniteTimeout);
      }

   public void Stop()
       {

           for(int i=0;i<waitHandleArray.Count;i++)
           {

               try
               {
                   waitHandleArray[i].Close();
               }
               catch
               {
                 
               }

           }

           try
           {
               // Specify to wait for all operations to return.
               WaitHandle.WaitAll(waitHandleArray.ToArray(),1000,false);
           }
           catch
           {
              
           }
          

       }
    }
}
