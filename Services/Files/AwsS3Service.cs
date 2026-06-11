using Amazon.S3;
using Amazon.S3.Model;
using TradePlatform.Api.DTOs;
using TradePlatform.Api.Services.Files;

public class AwsS3Service: IAwsS3Service
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;
    private readonly string _region;

    public AwsS3Service(IAmazonS3 s3, IConfiguration config)
    {
        _s3 = s3;
        _bucket = config["AWS:Bucket"];
        _region= config["AWS:Region"];
    }

    public string GetPreSignedUploadUrl(string key, string contentType)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.PUT,
            Expires = DateTime.UtcNow.AddMinutes(30),
            ContentType = contentType
        };

        return _s3.GetPreSignedURL(request);
    }

    public string GetPreSignedReadUrl(string key)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddHours(1)
        };

        // return _s3.GetPreSignedURL(request);
        return GetObjectUrl(key);
    }

    public string GetObjectUrl(string key)
    {
        return $"https://{_bucket}.s3.{_region}.amazonaws.com/{key}";
       // return $"https://{_bucket}.s3.amazonaws.com/{key}";
    }

    public async Task DeleteFileAsync(FileDeleteRequestDto dto)
    {
        await _s3.DeleteObjectAsync(_bucket, dto.file_name);
    }
}
