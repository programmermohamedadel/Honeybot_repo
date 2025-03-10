﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ransme
{
   public class AES
    {

        // [System.Runtime.Versioning.UnsupportedOSPlatform("browser")]
        //   public abstract class Aes : System.Security.Cryptography.SymmetricAlgorithm


        /*            using (Aes myAes = Aes.Create())
                    {


                 byte[] original = "Here is some data to encrypt!";
            // Encrypt the string to an array of bytes.
            byte[] encrypted = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);

            // Decrypt the bytes to a string.
            string roundtrip = DecryptStringFromBytes_Aes(encrypted, myAes.Key, myAes.IV);

            //Display the original data and the decrypted data.
            Console.WriteLine("Original:   {0}", original);
                        Console.WriteLine("Round Trip: {0}", roundtrip);
                    }
        */

        public static void EncryptFile(string filePath, byte[] key)
        {
            string tempFileName = Path.GetTempFileName();

            using (SymmetricAlgorithm cipher = Aes.Create())
            using (FileStream fileStream = File.OpenRead(filePath))
            using (FileStream tempFile = File.Create(tempFileName))
            {
                cipher.Key = key;
                // aes.IV will be automatically populated with a secure random value
                byte[] iv = cipher.IV;

                // Write a marker header so we can identify how to read this file in the future
                tempFile.WriteByte(69);
                tempFile.WriteByte(74);
                tempFile.WriteByte(66);
                tempFile.WriteByte(65);
                tempFile.WriteByte(69);
                tempFile.WriteByte(83);

                tempFile.Write(iv, 0, iv.Length);

                using (var cryptoStream =
                    new CryptoStream(tempFile, cipher.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    fileStream.CopyTo(cryptoStream);
                }
            }

            File.Delete(filePath);
            File.Move(tempFileName, filePath);
        }
/// <summary>
/// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/// </summary>
/// <param name="filePath"></param>
/// <param name="key"></param>
        public static void DecryptFile(string filePath, byte[] key)
        {
            string tempFileName = Path.GetTempFileName();

            using (SymmetricAlgorithm cipher = Aes.Create())
            using (FileStream fileStream = File.OpenRead(filePath))
            using (FileStream tempFile = File.Create(tempFileName))
            {
                cipher.Key = key;
                byte[] iv = new byte[cipher.BlockSize / 8];
                byte[] headerBytes = new byte[6];
                int remain = headerBytes.Length;

                while (remain != 0)
                {
                    int read = fileStream.Read(headerBytes, headerBytes.Length - remain, remain);

                    if (read == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    remain -= read;
                }

                if (headerBytes[0] != 69 ||
                    headerBytes[1] != 74 ||
                    headerBytes[2] != 66 ||
                    headerBytes[3] != 65 ||
                    headerBytes[4] != 69 ||
                    headerBytes[5] != 83)
                {
                    throw new InvalidOperationException();
                }

                remain = iv.Length;

                while (remain != 0)
                {
                    int read = fileStream.Read(iv, iv.Length - remain, remain);

                    if (read == 0)
                    {
                        throw new EndOfStreamException();
                    }

                    remain -= read;
                }

                cipher.IV = iv;

                using (var cryptoStream =
                    new CryptoStream(tempFile, cipher.CreateDecryptor(), CryptoStreamMode.Write))
                {
                    fileStream.CopyTo(cryptoStream);
                }
            }

            File.Delete(filePath);
            File.Move(tempFileName, filePath);
        }
    }
}
