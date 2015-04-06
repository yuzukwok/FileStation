using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FileStation.Util
{
    /// <summary>
    /// 生成随机字符
    /// </summary>
    public class DefaultRandomStringGenerator
    {
        
   
    private static  char[] PRINTABLE_CHARACTERS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ012345679".ToCharArray();
    

  
    private static  int DEFAULT_MAX_RANDOM_LENGTH = 35;


    private Random randomizer = new Random();

   
    private  int maximumRandomLength;

    public DefaultRandomStringGenerator() {
        this.maximumRandomLength = DEFAULT_MAX_RANDOM_LENGTH;
    }

    public DefaultRandomStringGenerator( int maxRandomLength) {
        this.maximumRandomLength = maxRandomLength;
    }

    public int getMinLength() {
        return this.maximumRandomLength;
    }

    public int getMaxLength() {
        return this.maximumRandomLength;
    }

    public String getNewString() {
         byte[] random = getNewStringAsBytes();

        return convertBytesToString(random);
    }


    public byte[] getNewStringAsBytes() {
         byte[] random = new byte[this.maximumRandomLength];

        this.randomizer.NextBytes(random);

        return random;
    }

    private String convertBytesToString( byte[] random) {
        char[] output = new char[random.Length];
        for (int i = 0; i < random.Length; i++) {
            int index = Math.Abs(random[i] % PRINTABLE_CHARACTERS.Length);
            output[i] = PRINTABLE_CHARACTERS[index];
        }

        return new String(output);
    }
    }
}