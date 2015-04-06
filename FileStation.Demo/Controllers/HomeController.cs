using FileStation.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileStation.Demo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

         [HttpPost]
        public ActionResult Upload(FormCollection form)
        {
            if (Request.Files.Count == 0)
            {
　　　　　　//Request.Files.Count 文件数为0上传不成功
　　　　　　return View();　
　　　　　}

            var file = Request.Files[0];
            if (file.ContentLength == 0)
            {
                //文件大小大（以字节为单位）为0时，做一些操作
                return View();
　　　　 }
　　　　 else
　　　　{
　　　　　　//文件大小不为0
　　　　　　HttpPostedFileBase fileData = Request.Files[0];
　　　　　　//保存成自己的文件全路径,newfile就是你上传后保存的文件,
　　　　　　//服务器上的UpLoadFile文件夹必须有读写权限　　　　　　
　　　　　//　file.SaveAs(Server.MapPath(@"Upload"));
      UploadHelper helper = new UploadHelper("Http://192.168.0.24/File/File/Upload");
                  string fileName = Path.GetFileName(fileData.FileName);// 原始文件名称
                    byte[] buffer = new byte[fileData.InputStream.Length + 1]; // 上传文件字节数组大小
                    fileData.InputStream.Read(buffer, 0, buffer.Length);  // 把上传文件的大小放入 buffer里
                  var re=  helper.Post("apptest", "", fileName, buffer);
                  return Content(re.FileId);
　　　　}
　　　　　
　　　　　
　　　　　　
            
            return View();

        }
    }
}
