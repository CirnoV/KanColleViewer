using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Grabacr07.KanColleViewer.QuestTracker.Models
{
	internal class RSA
	{
		private const string PublicKey = "MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQDNiwT/ab/kXuUQZdV3QrX9KNiAyNvjAHwjj/4/dBMrEyr/+hvjeiJLjqomjB3wwj5UYMBUm8gJ8OzJRSvuU0vOm9xDiQmxCZdCHv5cHYqqtIg4QlmOQWjxwgim1wZt8wixWcFkAkKiOlK6oOeh0GYeH0KnRLbEuh6XDKNAPmDQ5QIDAQAB";

		public static string Encrypt(string data)
		{
			byte[] bdata = Encoding.UTF8.GetBytes(data);
			var rsaPublic = CreateRsaProviderFromPublicKey(PublicKey);

			int blockSize = (rsaPublic.KeySize / 8) - 32;
			var buffer = new byte[blockSize];
			var list = new List<byte[]>();

			for (int i = 0; i < bdata.Length; i += blockSize)
			{
				buffer = new byte[Math.Min(blockSize, bdata.Length - i)];
				blockSize = buffer.Length;

				Buffer.BlockCopy(bdata, i, buffer, 0, blockSize);
				var encBuffer = rsaPublic.Encrypt(buffer, false);
				list.Add(encBuffer);
			}

			buffer = list.SelectMany(x => x).ToArray();
			return UrlSafeBase64Enc(buffer);
		}

		private static string UrlSafeBase64Enc(byte[] data)
		{
			var x = Convert.ToBase64String(data);
			x = x.Replace('+', '-');
			x = x.Replace('/', '_');
			return x.TrimEnd('=');
		}

		// https://gist.github.com/beginor/0d0acd7304c0e1d98d89e687aa8322e1
		private static RSACryptoServiceProvider CreateRsaProviderFromPublicKey(string publicKeyString)
		{
			// encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
			byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
			byte[] x509key;
			byte[] seq = new byte[15];
			int x509size;

			x509key = Convert.FromBase64String(publicKeyString);
			x509size = x509key.Length;

			// ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
			using (MemoryStream mem = new MemoryStream(x509key))
			{
				using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
				{
					byte bt = 0;
					ushort twobytes = 0;

					twobytes = binr.ReadUInt16();
					if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
						binr.ReadByte();    //advance 1 byte
					else if (twobytes == 0x8230)
						binr.ReadInt16();   //advance 2 bytes
					else
						return null;

					seq = binr.ReadBytes(15);       //read the Sequence OID
					if (!CompareBytearrays(seq, SeqOID))    //make sure Sequence for OID is correct
						return null;

					twobytes = binr.ReadUInt16();
					if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
						binr.ReadByte();    //advance 1 byte
					else if (twobytes == 0x8203)
						binr.ReadInt16();   //advance 2 bytes
					else
						return null;

					bt = binr.ReadByte();
					if (bt != 0x00)     //expect null byte next
						return null;

					twobytes = binr.ReadUInt16();
					if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
						binr.ReadByte();    //advance 1 byte
					else if (twobytes == 0x8230)
						binr.ReadInt16();   //advance 2 bytes
					else
						return null;

					twobytes = binr.ReadUInt16();
					byte lowbyte = 0x00;
					byte highbyte = 0x00;

					if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
						lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
					else if (twobytes == 0x8202)
					{
						highbyte = binr.ReadByte(); //advance 2 bytes
						lowbyte = binr.ReadByte();
					}
					else
						return null;
					byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
					int modsize = BitConverter.ToInt32(modint, 0);

					int firstbyte = binr.PeekChar();
					if (firstbyte == 0x00)
					{   //if first byte (highest order) of modulus is zero, don't include it
						binr.ReadByte();    //skip this null byte
						modsize -= 1;   //reduce modulus buffer size by 1
					}

					byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

					if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
						return null;
					int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
					byte[] exponent = binr.ReadBytes(expbytes);

					// ------- create RSACryptoServiceProvider instance and initialize with public key -----
					RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
					RSAParameters RSAKeyInfo = new RSAParameters();
					RSAKeyInfo.Modulus = modulus;
					RSAKeyInfo.Exponent = exponent;
					RSA.ImportParameters(RSAKeyInfo);

					return RSA;
				}

			}
		}

		private static bool CompareBytearrays(byte[] a, byte[] b)
		{
			if (a.Length != b.Length)
				return false;
			int i = 0;
			foreach (byte c in a)
			{
				if (c != b[i])
					return false;
				i++;
			}
			return true;
		}
	}
}
