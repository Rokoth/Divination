namespace Divination.Common
{
    public class Response<TResp> where TResp : class
    { 
        public ResponseEnum ResponseCode { get; set; }
        public TResp ResponseBody { get; set; }
    }
}