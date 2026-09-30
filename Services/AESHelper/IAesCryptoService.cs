namespace TradePlatform.Api.Services.AESHelper;

public interface IAesCryptoService
{
	string Encrypt(string plainText);

	string Decrypt(string cipherTextBase64);
}
