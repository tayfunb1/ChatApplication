namespace ChatApplication.Business.Models.Common;

public class BaseResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ResponseCodes ResponseCode { get; set; }
}

public class BaseApiResponse<T> : BaseResponse
{
    public T Data { get; set; }
}

public class BaseApiListResponse<T> : BaseResponse
{
    public List<T> DataList { get; set; }
    public int Count => DataList?.Count ?? 0;
}

