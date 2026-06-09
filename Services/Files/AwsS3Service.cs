using Amazon.S3;
using Amazon.S3.Model;

public class AwsS3Service
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public AwsS3Service(IAmazonS3 s3, IConfiguration config)
    {
        _s3 = s3;
        _bucket = config["AWS:BucketName"];
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

        return _s3.GetPreSignedURL(request);
    }

    public string GetObjectUrl(string key)
    {
        return $"https://{_bucket}.s3.amazonaws.com/{key}";
    }

    public async Task DeleteObjectAsync(string key)
    {
        await _s3.DeleteObjectAsync(_bucket, key);
    }
}
