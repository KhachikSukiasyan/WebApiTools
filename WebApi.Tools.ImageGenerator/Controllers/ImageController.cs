using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApi.Tools.ImageGenerator.Controllers
{
    public class ImageController : ApiController
    {
        [Route("img/{width:int}x{height:int}/{bgColor}/{fgColor}")]
        public async Task<HttpResponseMessage> Get(int width, int height, string bgColor, string fgColor)
        {
            var bg = GetColor(bgColor, Color.LightGray);
            var fg = GetColor(fgColor, Color.DarkGray);

            var result = new HttpResponseMessage(HttpStatusCode.OK);

            using (var ms = new MemoryStream())
            using (var img = await GetImageAsync($"{width} ⨯ {height}", width, height, bg, fg))
            {
                if (img == null)
                {
                    result.StatusCode = HttpStatusCode.BadRequest;
                    return result;
                }

                img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                var bytes = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                await ms.ReadAsync(bytes, 0, bytes.Length);

                result.Content = new ByteArrayContent(bytes);
            }

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            return result;
        }

        private Color GetColor(string colorToParse, Color defaultColor)
        {
            try
            {
                return ColorTranslator.FromHtml($"#{colorToParse}");
            }
            catch (Exception)
            {
                return defaultColor;
            }
        }

        private Task<Image> GetImageAsync(string text, int width, int height, Color bgColor, Color fgColor)
        {
            return Task.Run(() =>
            {
                Image img;

                try
                {
                    img = new Bitmap(width, height);
                }
                catch
                {
                    return null;
                }

                using (var graphics = Graphics.FromImage(img))
                using (var brush = new SolidBrush(fgColor))
                using (var stringFormat = new StringFormat())
                using (var font = new Font("Arial", 40, FontStyle.Bold, GraphicsUnit.Point))
                {
                    graphics.Clear(bgColor);

                    Rectangle rect = new Rectangle(0, 0, width - 1, height - 1);

                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    graphics.DrawString(text, font, brush, rect, stringFormat);
                }

                return img;
            });
        }
    }
}
