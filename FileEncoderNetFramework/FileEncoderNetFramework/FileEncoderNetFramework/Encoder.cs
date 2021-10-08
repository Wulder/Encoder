using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using System.IO;

namespace FileEncoderNetFramework
{
    class Encoder
    {
        RSA rsa;
        bool IsPrivateKey;

        public static Encoder CurrentEncoder;

        public Encoder()
        {
            GenerateKeys();
            CurrentEncoder = this;
 
        }

        public void GenerateKeys()
        {
            rsa = RSA.Create();
            IsPrivateKey = true;
            Commands.ColorWriteLine($"Ключи пуспешно обновлены!", ConsoleColor.Green);
        }
        public byte[] Encode(byte[] data)
        {
           
            try
            {
                return rsa.Encrypt(data, RSAEncryptionPadding.OaepSHA1);
            }
            catch(Exception e)
            {
                Commands.ColorWriteLine(e.ToString(), ConsoleColor.Red);
                return new byte[0];
            }
        }

        public byte[] Decode(byte[] encodedData)
        {
            try
            {
                return rsa.Decrypt(encodedData, RSAEncryptionPadding.OaepSHA1);
            }
            catch (Exception e)
            {
               Commands.ColorWriteLine(e.ToString(), ConsoleColor.Red);
                return new byte[0];
            }
        }

        public void GetInfo()
        {
            RSAParameters p;
            try
            {
                p = rsa.ExportParameters(true);
            }
            catch
            {
                
                p = rsa.ExportParameters(false);
            }
            
            Console.WriteLine($"Q: {Convert(p.Q)}\n" +
                $"P: {Convert(p.P)}\n" +
                $"Module: {Convert(p.Modulus)}\n" +
                $"Private Exponent: {Convert(p.D)}\n" +
                $"Public Exponent: {Convert(p.Exponent)}\n" +
                $"KeySize: {rsa.KeySize} bit\n");
           

            BigInteger Convert(byte[] b)
            {
                if (b != null)
                    return new BigInteger(b);
                else
                    return 0;
            }
        }

        BinaryFormatter formatter = new BinaryFormatter();
        public void SaveKeyFile(string SaveDir,string keyName, bool privateKey)
        {
            try
            {
                FileStream fs = new FileStream(SaveDir + $"\\{keyName}.key", FileMode.OpenOrCreate);
                SerializableKey k = new SerializableKey();
                k = k.GenerateKey(rsa.ExportParameters(privateKey));
                formatter.Serialize(fs, k);
                fs.Close();
            }
            catch(Exception e)
            {
                Commands.ColorWriteLine($"{e}", ConsoleColor.Red);
            }
        }
        

        public void LoadKeyFile(string fileDir)
        {
            try
            {
                FileStream fs = new FileStream(fileDir, FileMode.Open);
                
                SerializableKey k = (SerializableKey)formatter.Deserialize(fs);
                IsPrivateKey = k.D == null ? false : true;
                rsa.ImportParameters(k.GetRSAParameters(k));
                if (IsPrivateKey)
                    Commands.ColorWriteLine("Приватный ключ успешно загружен!", ConsoleColor.Yellow);
                else
                    Commands.ColorWriteLine("Публичный ключ успешно загружен!", ConsoleColor.Yellow);

            }
            catch (Exception e)
            {
                Commands.ColorWriteLine($"{e}", ConsoleColor.Red);
            }
        }


        public void EncodeFile(string FileDir, int DataFrameSize, bool CopyEncodedFile, bool copyfile)
        {
            DateTime start = DateTime.Now;
            byte[] FileBytes = File.ReadAllBytes(FileDir);
            List<byte> EncodedBytes = new List<byte>();

            int FramesEncoded = 0;
            bool Reading = true;
            while(Reading)
            {
                byte[] Frame = new byte[DataFrameSize];
                Commands.ColorWriteLine($"Encoding frame {FramesEncoded}... Completed {EncodedBytes.Count/1024} KB",ConsoleColor.Yellow);
                for (int i = 0; i < DataFrameSize; i++)
                {
                    if (FramesEncoded * DataFrameSize + i < FileBytes.Length)
                    {
                        Frame[i] = FileBytes[FramesEncoded * DataFrameSize + i];
                    }
                    else
                    {
                        Reading = false;
                        break;
                    }
                }
                AddRange(Encode(Frame));
                FramesEncoded++;
            }

            if (copyfile)
                File.WriteAllBytes(Path.GetDirectoryName(FileDir) +$"\\{Path.GetFileNameWithoutExtension(FileDir)}CopyCoded{Path.GetExtension(FileDir)}", EncodedBytes.ToArray());   
            else
                File.WriteAllBytes(FileDir, EncodedBytes.ToArray());

            Commands.ColorWriteLine($"Шифрование файла успешно завершено!({(DateTime.Now - start).TotalMilliseconds.ToString("0")} ms)",ConsoleColor.Green);


            void AddRange(byte[] r)
            {
                for (int i = 0; i < r.Length; i++)
                    EncodedBytes.Add(r[i]);
            }
        }

        public void DecodeFile(string FileDir, int EncoderFrameLength, bool CopyDecodedFile, bool copyfile)
        {
           

            DateTime start = DateTime.Now;
            byte[] EncodedFileBytes = File.ReadAllBytes(FileDir);
            List<byte> DecodedBytes = new List<byte>();

            if(EncodedFileBytes.Length%EncoderFrameLength == 0)
            {
                for(int i = 0; i < EncodedFileBytes.Length/ EncoderFrameLength; i++)
                {
                    Commands.ColorWriteLine($"Decoding {i} frame... Complete {DecodedBytes.Count/1024}KB", ConsoleColor.Yellow);
                    byte[] DataFrame = new byte[EncoderFrameLength];
                    Array.Copy(EncodedFileBytes, i * EncoderFrameLength, DataFrame, 0, EncoderFrameLength);
                    FillArray(Decode(DataFrame));
                }

                if (copyfile)
                    File.WriteAllBytes(Path.GetDirectoryName(FileDir) + $"\\{Path.GetFileNameWithoutExtension(FileDir)}CopyDecoded{Path.GetExtension(FileDir)}", DecodedBytes.ToArray());
                else
                    File.WriteAllBytes(FileDir, DecodedBytes.ToArray());
                Commands.ColorWriteLine($"Шифрование файла успешно завершено!({(DateTime.Now - start).TotalMilliseconds.ToString("0")} ms)", ConsoleColor.Green);
            }
            else
            {
                Commands.ColorWriteLine($"Файл имеет не правильную длинну (должен быть картным {EncoderFrameLength})", ConsoleColor.Red);
            }

            void FillArray(byte[] arr)
            {
                for (int j = 0; j < arr.Length; j++)
                    DecodedBytes.Add(arr[j]);
            }
     
        }

        
        
        public static void WriteBytes(byte[] b, bool Devider)
        {
            for(int i = 0; i < b.Length; i++)
            {
                Console.WriteLine($"Byte {(Devider ? i % 128 : i)} = {b[i]}");
            }
        }

        [Serializable]
        public struct SerializableKey
        {
            
            public byte[] Exponent;
            
            public byte[] Modulus;
            
            public byte[] P;
           
            public byte[] Q;
            
            public byte[] DP;
           
            public byte[] DQ;
            
            public byte[] InverseQ;
           
            public byte[] D;

            public RSAParameters GetRSAParameters(SerializableKey k)
            {
                RSAParameters p = new RSAParameters();
                p.Modulus = k.Modulus;
                p.Exponent = k.Exponent;
                p.P = k.P;
                p.Q = k.Q;
                p.DP = k.DP;
                p.DQ = k.DQ;
                p.InverseQ = k.InverseQ;
                p.D = k.D;
                return p;
            }

            public SerializableKey GenerateKey(RSAParameters p)
            {
                SerializableKey k = new SerializableKey();
                k.Modulus = p.Modulus;
                k.Exponent = p.Exponent;
                k.P = p.P;
                k.Q = p.Q;
                k.DP = p.DP;
                k.DQ = p.DQ;
                k.InverseQ = p.InverseQ;
                k.D = p.D;
                return k;
            }
        }

    }
}
