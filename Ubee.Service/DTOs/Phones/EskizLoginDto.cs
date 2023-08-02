namespace Ubee.Service.DTOs.Phones
{
    public class EskizLoginDto
    {
        public string Message { get; set; } = String.Empty;

        public EskizToken Data { get; set; }

        public EskizLoginDto()
        {
            Data = new EskizToken();
        }

        public class EskizToken
        {
            public string Token { get; set; } = String.Empty;
        }
    }
}
