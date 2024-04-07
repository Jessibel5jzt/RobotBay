using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 自定义的扩展方法：必须是在静态类中
	/// </summary>
	public static class Util
	{
		/// <summary>
		/// 根据GUID获取唯一字符串
		/// </summary>
		/// <param name=\"guid\"></param>
		/// <returns></returns>
		public static string GuidTo16String()
		{
			long i = 1;
			foreach (byte b in Guid.NewGuid().ToByteArray())
				i *= ((int)b + 1);
			return string.Format("{0:x}", i - DateTime.Now.Ticks) + DateTime.Now.ToString("yyyyMMddHHmmssff"); ;
		}

		public static string FileMD5(string filePath)
		{
			string retVal;
			using (FileStream file = new FileStream(filePath, FileMode.Open))
			{
				MD5 md5 = new MD5CryptoServiceProvider();
				retVal = BitConverter.ToString(md5.ComputeHash(file), 4, 8);
			}

			return retVal.Replace("-", "");
		}

		/// <summary>
		/// 字符串转时间
		/// </summary>
		/// <param name="dateStr"></param>
		/// <returns></returns>
		public static DateTime ParseDateTime(string dateStr)
		{
			DateTime result = DateTime.Now;
			try
			{
				result = DateTime.Parse(dateStr);
			}
			catch
			{
			}

			return result;
		}

		/// <summary>
		/// 用RSA公钥 加密
		/// </summary>
		/// <param name="data"></param>
		/// <param name="publicKey"></param>
		/// <returns></returns>
		public static string RSAEncrypt(string password, string publicKey)
		{
			System.Security.Cryptography.RSACryptoServiceProvider rsa = new System.Security.Cryptography.RSACryptoServiceProvider();
			rsa.FromXmlString(publicKey);
			byte[] data = new UnicodeEncoding().GetBytes(password);
			byte[] encryptData = rsa.Encrypt(data, false);
			string safepassword = Convert.ToBase64String(encryptData);
			return safepassword;
		}

		/// <summary>
		/// SHA1加密算法
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public static string GetSHA1Password(string password)
		{
			byte[] bytes = Encoding.UTF7.GetBytes(password);
			byte[] result;
			System.Security.Cryptography.SHA1 shaM = new System.Security.Cryptography.SHA1Managed();
			result = shaM.ComputeHash(bytes);
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < result.Length; i++)
			{
				sb.AppendFormat("{0:x2}", result[i]);
			}

			return sb.ToString();
		}

		/// <summary>
		/// 是否是手机号
		/// </summary>
		/// <param name="content"></param>
		/// <param name="len"></param>
		/// <returns></returns>
		public static bool IsMobile(string content, int len = 11)
		{
			if (content == null) return false;
			if (content.Length != len) return false;

			Regex regex = new Regex(@"^[0-9]*$");
			return regex.IsMatch(content);
		}

		/// <summary>
		/// 是否是邮箱
		/// </summary>
		/// <param name="content"></param>
		/// <returns></returns>
		public static bool IsEmail(string content)
		{
			if (content == null) return false;

			Regex regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
			return regex.IsMatch(content);
		}

		/// <summary>
		/// Sprite转Texture2D
		/// </summary>
		/// <param name="sprite"></param>
		/// <returns></returns>
		public static Texture2D ConvertSprite2Tex(Sprite sprite)
		{
			Texture2D result = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
			result.name = sprite.name;
			var pixels = sprite.texture.GetPixels(
				(int)sprite.textureRect.x,
				(int)sprite.textureRect.y,
				(int)sprite.textureRect.width,
				(int)sprite.textureRect.height);
			result.SetPixels(pixels);
			result.Apply();
			return result;
		}
	}
}