using System;
using System.IO;
using System.Security.Cryptography;

/*
DES加解密类
    加密操作
        输入：64比特数据，64比特密钥
        输出：64比特密文
    解密操作
        输入：64比特密文，64比特密钥
        输出：64比特数据
 */
namespace myChatRoom
{
    public class DESCrypt
    {
        public string Encrypt(string message, byte[] key)
        {
            // if(message.Length!=8||key.Length!=8){
            //     return null;
            // }
            byte[] byteMessage = System.Text.Encoding.ASCII.GetBytes(message);
            // byte[] byteKey=System.Text.Encoding.ASCII.GetBytes(key);
            return Encrypt(byteMessage, key);
            // DESCryptoServiceProvider desService=new DESCryptoServiceProvider();
            // desService.Key=byteKey;
            // desService.IV=byteKey;
            // MemoryStream ms=new MemoryStream();
            // CryptoStream cs=new CryptoStream(ms,desService.CreateEncryptor(),CryptoStreamMode.Write);
            // cs.Write(byteMessage,0,byteMessage.Length);
            // cs.FlushFinalBlock();
            // // char[] result=new char[Convert.];
            // // Convert.ToBase64CharArray(ms.ToArray(),0,ms.ToArray().Length,result,0);
            // return Convert.ToBase64String(ms.ToArray());

        }
        public string Encrypt(byte[] message, byte[] key)
        {
            DESCryptoServiceProvider desService = new DESCryptoServiceProvider();
            desService.Key = key;
            desService.IV = key;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, desService.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(message, 0, message.Length);
            cs.FlushFinalBlock();
            // char[] result=new char[Convert.];
            // Convert.ToBase64CharArray(ms.ToArray(),0,ms.ToArray().Length,result,0);
            return Convert.ToBase64String(ms.ToArray());

        }
        public string Decrypt(string cipher, byte[] key)
        {
            // if(cipher.Length!=8||key.Length()!=8){
            //     return null;
            // }

            byte[] byteCipher = Convert.FromBase64String(cipher);
            // byte[] byteKey=System.Text.Encoding.ASCII.GetBytes(key);
            DESCryptoServiceProvider desService = new DESCryptoServiceProvider();
            desService.Key = key;
            desService.IV = key;
            MemoryStream ms = new MemoryStream();
            CryptoStream cs = new CryptoStream(ms, desService.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(byteCipher, 0, byteCipher.Length);
            cs.FlushFinalBlock();
            return System.Text.Encoding.ASCII.GetString(ms.ToArray());
        }
        // 测试用
        // public static void Main(){
        //     char[] message="whitehatandblackhat".ToCharArray();
        //     char[] key="blackhat".ToCharArray();
        //     DESCrypt des=new DESCrypt();
        //     string cipher=des.Encrypt(message,key);
        //     System.Console.WriteLine(cipher);
        //     System.Console.WriteLine(des.Decrypt(cipher,key));
        // }
    }
}