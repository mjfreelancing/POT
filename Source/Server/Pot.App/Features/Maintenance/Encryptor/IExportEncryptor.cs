using Pot.Shared.DependencyInjection;

namespace Pot.App.Features.Maintenance.Encryptor;

public interface IExportEncryptor : IPotSingletonDependency
{
    byte[] Encrypt(string publicKey, byte[] data);
    Stream Decrypt(string publicKey, Stream cipherText);
}
