using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace FileStation.Util
{
    public class FileIdGenerator
    {

       

        private DefaultRandomStringGenerator randomStringGenerator;


       




        public FileIdGenerator()
        {          
            this.randomStringGenerator = new DefaultRandomStringGenerator(10);
        }



        public String getFileId()
        {
          
            StringBuilder buffer = new StringBuilder( this.randomStringGenerator.getMaxLength());

            buffer.Append(this.randomStringGenerator.getNewString());

            return buffer.ToString();
        }


    }
}