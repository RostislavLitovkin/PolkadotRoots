using Substrate.NET.Wallet;
using Substrate.NET.Wallet.Keyring;
using Substrate.NetApi.Extensions;
using static Substrate.NetApi.Mnemonic;

namespace CommunityTests
{
    public class Helpers
    {
        public static (string address, Wallet wallet) GenerateAccount()
        {
            var meta = new Meta() { Name = "PlutoFramework" };
            var mnemonics = MnemonicFromEntropy(new byte[16].Populate(), BIP39Wordlist.English);
            var keyring = new Keyring();
            var wallet = keyring.AddFromMnemonic(mnemonics, meta, Substrate.NetApi.Model.Types.KeyType.Sr25519);
            return (wallet.Account.Value, wallet);
        }
    }
}
