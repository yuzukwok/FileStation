using FileStation.SDK;
using FileStation.Util;
using FileStation.Util.Manager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FileStation.Controllers
{
    public class FileController : Controller
    {
     
      
            public JsonResult Upload()
            {
                //当磁盘空间不足时，整个服务不可用
                if (!string.IsNullOrEmpty(FSCenter.ErrMsg))
                {

                    return Json(new UploadResult() { Result = UploadState.Err_DiskFull });
                }
                
                var context = ControllerContext.HttpContext;
               
             
                var provider = (IServiceProvider)context;
                var workerRequest = (HttpWorkerRequest)provider.GetService(typeof(HttpWorkerRequest));
               
                var verb = workerRequest.GetHttpVerbName();
                //判断谓词
                if (!verb.Equals("POST"))
                {
                   
                    return Json(new UploadResult() { Result = UploadState.Err_MustPostVerb });
                }
                //判断剩余空间
                // 取得文件大小
                int filesize = workerRequest.GetTotalEntityBodyLength();
                if ((FSCenter.CurrentDiskMax- FSCenter.CurrentDisk+(ulong)filesize)<(ulong)FSCenter.MinFreeDiskSize)
                {
                    //分区磁盘不够
                    try
                    {
                        FSCenter.ChangeDisk();
                    }
                    catch (Exception)
                    {

                        return Json(new UploadResult() { Result = UploadState.Err_DiskFull });
                    }
                  
                   
                }
                
               
                //取得MIME类型
                string  mime="";
                //取得文件扩展名
                string ext = "";

                //确定当前文件夹数目是否超过限制
                if (FSCenter.CurrentFileCount>FSCenter.FileCountMax)
                {
                    FSCenter.ChangeFolder();
                }

                //生成FileId
                string random = FSCenter.FileIdGenerator.getFileId();
                string fileid = "/" + FSCenter.FirstFolder + "/" + FSCenter.SecondFolder + "/" +random  + ext;
                string filepath = FSCenter.CurrentPath + @"\" + random + ext;

                var encoding = context.Request.ContentEncoding;
                var processor = new UploadProcessor(workerRequest);
                //取得当前原始文件名
                string filename = "";
               
                try
                {
                    processor.StreamToDisk(context, encoding, filepath,ref filename,ref ext,ref  mime);
                }
                catch (Exception ex)
                {
                   
                    return Json(new UploadResult() { Result = UploadState.Err_UploadFaild });
                }

              
                fileid = fileid + ext;
                filepath = filepath + ext;
                var file = new FileInfo(filepath);
                //一旦在保存前读取了RequestForm则会出错
                var key = context.Request.Form["passkey"];

                //验证来源
                var checkkey=FSCenter.Vaild(key);
                if (!checkkey)
                {
                    //删除文件
                    file.Delete();
                    //返回错误
                     return Json(new UploadResult() { Result = UploadState.Err_InVaildPassKey });
                }

                string op="Add";
                //是否存在旧id
                var oldfile = context.Request.Form["oldfileid"];
                //解析成文件地址
                if (!string.IsNullOrEmpty(oldfile))
                {
                    //取得虚拟目录
                    var list = oldfile.Split('/');
                    if (list.Length==4)
                    { //移动更新文件
                     var  pn=   FSCenter.GetPhsyicPath(list[1], list[2]);
                     file.MoveTo(pn + "\\" + list[3]);
                        //名字更新
                        fileid = oldfile;
                        op = "Update";
                    }
                   
                }

                try
                {
                    //保存元数据到数据//要异步

                   FSCenter.SaveInfo(fileid,key,filename,mime,ext,filesize,op);

                }
                catch (Exception)
                {
                    
                   //
                }
              
                try
                {
                    //发送到消息队列

                    FSCenter.SendMQInfo(fileid, oldfile, op);

                }
                catch (Exception)
                {

                    //
                }

                //更新当前计数
                FSCenter.CurrentFileCount++;
                FSCenter.CurrentDisk +=(ulong) filesize;

                return Json(new UploadResult() { Result = UploadState.OK, FileId=fileid });
            }
        }

    }

