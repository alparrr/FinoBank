using System.Numerics;
using System.Text;


namespace FinoBankApi.Helpers
{
    public static class AccountNumberGenerator
    {
        public static string Generate()
        {
            var random = new Random();
            
            string bankId = "10101010"; 
            
            var clientNumberBuilder = new StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                clientNumberBuilder.Append(random.Next(0, 10));
            }
            string clientNumber = clientNumberBuilder.ToString();

            string input = bankId + clientNumber + "2521" + "00";
            
            BigInteger number = BigInteger.Parse(input);
            int modulo = (int)(number % 97);
            int checksum = 98 - modulo;

            string checksumStr = checksum < 10 ? "0" + checksum : checksum.ToString();

            return "PL" + checksumStr + bankId + clientNumber;
        }
    }
}