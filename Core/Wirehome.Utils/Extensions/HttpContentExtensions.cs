using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Wirehome.Core.Extensions
{
    public static class HttpContentExtensions
    {
        public static async Task<string> ReadAsStringAsync(this HttpContent content, Encoding encoding)
        {
            var responseStream = await content.ReadAsStreamAsync().ConfigureAwait(false);
            var responseContent = string.Empty;

            if (encoding == null) encoding = content.GetEncoding(Encoding.UTF8);

            using (var sr = new StreamReader(responseStream, encoding))
            {
                responseContent = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            return responseContent;
        }

        public static Encoding GetEncoding(this HttpContent content, Encoding defaultEncoding)
        {
            var encoding = defaultEncoding;
            var charset = content.Headers.ContentType.CharSet;
            if (!string.IsNullOrEmpty(charset))
            {
                try
                {
                    encoding = Encoding.GetEncoding(charset);
                }
                catch
                {
                    encoding = defaultEncoding;
                }
            }

            return encoding;
        }
    }
}