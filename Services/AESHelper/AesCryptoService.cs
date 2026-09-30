using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TradePlatform.Api.Models.MagicPayLoad;

namespace TradePlatform.Api.Services.AESHelper;

public class AesCryptoService : IAesCryptoService
{
	private readonly byte[] _key;

	private readonly byte[] _iv;

	public AesCryptoService(IOptions<MagicLoginSettings> options)
	{
		_key = Encoding.UTF8.GetBytes(options.Value.Key);
		_iv = Encoding.UTF8.GetBytes(options.Value.IV);
	}

	public string Encrypt(string plainText)
	{
		using Aes aes = Aes.Create();
		aes.Key = _key;
		aes.IV = _iv;
		using ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
		using MemoryStream ms = new MemoryStream();
		using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
		using StreamWriter sw = new StreamWriter(cs);
		sw.Write(plainText);
		sw.Close();
		return Convert.ToBase64String(ms.ToArray());
	}

	public string Decrypt(string cipherTextBase64)
	{
		byte[] buffer = Convert.FromBase64String(cipherTextBase64);
		using Aes aes = Aes.Create();
		aes.Key = _key;
		aes.IV = _iv;
		using ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
		using MemoryStream ms = new MemoryStream(buffer);
		using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
		using StreamReader sr = new StreamReader(cs);
		return sr.ReadToEnd();
	}
}
