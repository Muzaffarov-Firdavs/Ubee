namespace Ubee.Service.Helpers
{
    public class CodeGenerator
    {
        public static int GenerateRandomCode()
        {
            Random random = new Random();
            return random.Next(10000,99999);
        }
    }
}
