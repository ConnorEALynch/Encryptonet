using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using System.Configuration;

namespace Encryptonet_Sockets
{
    class Program
    {
        public enum mode { decrypt, encrypt};
        static mode operation;
        public static List<byte> KEY;
        public static List<byte> IV;
        static string filePath = "";
        static string fileName = "";
        static string fileExtension = "";
        static void Main(string[] args)
        {

            if (ParseArgs(args) == true)
            {
                if (File.Exists(filePath))
                {
                    byte[] fileContents = ReadFile();
                    switch (operation)
                    {
                        case mode.decrypt:
                            using (FileStream fsDecrypt = new FileStream(fileName, FileMode.OpenOrCreate))
                            {
                                using (CryptoStream csDecrypt = Decrypt(fsDecrypt))
                                {
                                    using (BinaryWriter srDecrypt = new BinaryWriter(csDecrypt))
                                    {

                                        // Read the decrypted bytes from the decrypting stream
                                        // and place them in a string.
                                        srDecrypt.Write(fileContents);
                                    }
                                }
                            }
                            break;

                        case mode.encrypt:
                            //Create a file stream
                            using (FileStream fStream = new FileStream(fileName + ".aes", FileMode.OpenOrCreate))
                            {
                                using (CryptoStream cStream = Encrypt(fStream))
                                {
                                    //maybe use switch statemant
                                    if (cStream != null)
                                    {
                                        //continue

                                        using (BinaryWriter bWriter = new BinaryWriter(cStream, Encoding.UTF8))
                                        {

                                            //Write to the stream.  
                                            bWriter.Write(fileContents);
                                        }
                                    }
                                    else
                                    {

                                    }
                                    
                                }
                            }

                            break;     
                    }


                    
                }
                else
                {
                    Logger.LogMessage("File Does not exist");
                }
            }
            else
            {
                Logger.LogMessage("check command line arguements, inputs were: ");
            }
        }
        static List<byte> parseConfigArray(string[] values)
        {
            List<byte> result = new List<byte>();
            foreach (string value in values)
            {
                result.Add(Byte.Parse(value));
            }
            return result;
        }
        
        static bool ParseArgs(string[] args)
        {
            //read and parse Key and IV from config file
            IV = parseConfigArray(ConfigurationManager.AppSettings["IVs"].Split(','));
            KEY = parseConfigArray(ConfigurationManager.AppSettings["Keys"].Split(','));
           
            bool result = false;
            if (args.Length > 0 && args.Length <= 2)
            {
                foreach(string arg in args )
                {
                    if (arg[0] == '-')
                    {
                        switch (arg[1]) {
                            case'd':
                                operation = mode.decrypt;
                                result = true;
                                break;
                            case 'e':
                                operation = mode.encrypt;
                                result = true;
                                break;
                            default:
                                Logger.LogMessage("no operation flag chosen");
                                break;
                        }
                    }
                    else
                    {
                        filePath = Path.GetFullPath(arg);
                        fileName = Path.GetFileNameWithoutExtension(arg);
                        fileExtension = Path.GetExtension(arg);
                    }
                }
                
            }
            //expand on this with key and ID selcetion
            return result;
        }
        static byte[] ReadFile()
        {
            var bytes = default(byte[]);
            StreamReader reader = new StreamReader(filePath);
            using (var memstream = new MemoryStream())
            {
                reader.BaseStream.CopyTo(memstream);
                bytes = memstream.ToArray();
            }
            return bytes;
        }

        /*
         * FUNCTION:    Encrypt(Steram stream)
         * DESCIOPTION: encrypts the strem given to it and returns a Crypto stream
         * PARAMETERS:  Stream stream
         * Stream stream: the stream to be encrypted
         * 
         */
        static CryptoStream Encrypt(Stream stream)
        {
            CryptoStream cStream = null;
            try
            {


                //Create a new instance of the default Aes implementation class  
                // and encrypt the stream.  

                //this needs to abstracted and key/iv need to be changable
                using (Aes aes = Aes.Create())
                {



                    //Create a CryptoStream, pass it the FileStream, and encrypt
                    //it with the Aes class.  
                    cStream = new CryptoStream(
                       stream,
                       aes.CreateEncryptor(KEY.ToArray(), IV.ToArray()),
                       CryptoStreamMode.Write);
                }
            }
            catch (ArgumentException argExcept)
            {
                //check args
                Logger.LogError(argExcept);
            }
            catch(Exception exception)
            {
                Logger.LogError(exception);
            }
            return cStream;
        }

        static CryptoStream Decrypt(Stream stream)
        {
            CryptoStream cStream = null;
            try
            {

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = KEY.ToArray();
                    aesAlg.IV = IV.ToArray();

                    // Create a decryptor to perform the stream transform.
                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);


                        //Create a CryptoStream, pass it the FileStream, and encrypt
                        //it with the Aes class.  
                        cStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Write);
                    
                }
            }
            catch (ArgumentException argExcept)
            {
                //check args
                Logger.LogError(argExcept);
            }
            catch (Exception exception)
            {
                Logger.LogError(exception);
            }
            return cStream;
        }
    }
}
