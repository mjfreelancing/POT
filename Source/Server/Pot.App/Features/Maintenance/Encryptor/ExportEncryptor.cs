using AllOverIt.Cryptography.RSA;
using Microsoft.Extensions.Configuration;

namespace Pot.App.Features.Maintenance.Encryptor;

internal sealed class ExportEncryptor : IExportEncryptor
{
    private readonly string _privateKey;

    public ExportEncryptor(IConfiguration configuration)
    {
        // Effectively the same as configuration["RSA__PRIVATE_KEY"] when an environment variable is set
        _privateKey = configuration["Rsa:PrivateKey"] ?? throw new InvalidOperationException("The RSA private key is not defined.");
    }

    public byte[] Encrypt(string publicKey, byte[] data)
    {
        var rsaKeyPair = new RsaKeyPair(publicKey, _privateKey);    // TODO: Create a factory that has the private key, is given the public key, returns the encryptor and block size
        var rsaEncryptor = new RsaEncryptor(rsaKeyPair);

        var maxLength = rsaEncryptor.GetMaxInputLength();
        using var output = new MemoryStream();

        for (int offset = 0; offset < data.Length; offset += maxLength)
        {
            int chunkSize = Math.Min(maxLength, data.Length - offset);
            var chunk = new byte[chunkSize];
            Array.Copy(data, offset, chunk, 0, chunkSize);

            var encryptedChunk = rsaEncryptor.Encrypt(chunk);
            output.Write(encryptedChunk, 0, encryptedChunk.Length);
        }

        return output.ToArray();
    }

    public Stream Decrypt(string publicKey, Stream cipherText)
    {
        var rsaKeyPair = new RsaKeyPair(publicKey, _privateKey);
        var rsaEncryptor = new RsaEncryptor(rsaKeyPair);
        var blockSize = rsaKeyPair.KeySize / 8;

        var memoryStream = new MemoryStream();
        var buffer = new byte[blockSize];
        int bytesRead;

        // ZipArchiveEntry streams may not return the requested block size, so we need to explicitly read full blocks.
        while ((bytesRead = ReadFullBlock(cipherText, buffer, blockSize)) > 0)
        {
            if (bytesRead != blockSize)
            {
                throw new InvalidOperationException($"Failed to decrypt stream. Data may be corrupted.");
            }

            var decryptedChunk = rsaEncryptor.Decrypt(buffer);
            memoryStream.Write(decryptedChunk, 0, decryptedChunk.Length);
        }

        memoryStream.Flush();
        memoryStream.Position = 0;

        return memoryStream;
    }

    private static int ReadFullBlock(Stream stream, byte[] buffer, int blockSize)
    {
        int totalRead = 0;

        while (totalRead < blockSize)
        {
            int bytesRead = stream.Read(buffer, totalRead, blockSize - totalRead);

            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;
        }

        return totalRead;
    }
}
