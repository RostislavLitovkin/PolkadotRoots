using Substrate.NET.Wallet.Keyring;
using Substrate.NetApi.Extensions;
using Substrate.NetApi.Model.Types;
using static Substrate.NetApi.Mnemonic;

namespace CommunityTests
{
    public class Helpers
    {
        public static Account GenerateAccount()
        {
            var meta = new Meta() { Name = "PlutoFramework" };
            var mnemonics = MnemonicFromEntropy(new byte[16].Populate(), BIP39Wordlist.English);
            var keyring = new Keyring();
            var wallet = keyring.AddFromMnemonic(mnemonics, meta, KeyType.Sr25519);
            return wallet.Account;
        }

        public static Account GenerateAdmin()
        {
            var meta = new Meta() { Name = "PlutoFramework" };
            var mnemonics = "//admin";
            var keyring = new Keyring();
            var wallet = keyring.AddFromMnemonic(mnemonics, meta, KeyType.Sr25519);

            return wallet.Account;
        }
    }

}
