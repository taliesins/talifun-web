using System.Drawing;
using System.IO;
using System.Net;

namespace Talifun.Web.CssSprite
{
    public class SpriteElement
    {
        /// <summary>
        /// Gets the sprite image from a stream
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stream"></param>
        public SpriteElement(string name, Stream stream)
        {
            Name = name;
            var image = (Bitmap)Bitmap.FromStream(stream);
            Image = image;
        }
        
        /// <summary>
        /// Gets the sprite image from a bitmap
        /// </summary>
        /// <param name="name"></param>
        /// <param name="image"></param>
        public SpriteElement(string name, Bitmap image)
        {
            Name = name;
            Image = image;
        }

        /// <summary>
        /// Gets the sprite image from a url
        /// </summary>
        /// <param name="name"></param>
        /// <param name="absoluteUrl"></param>
        public SpriteElement(string name, string absoluteUrl)
        {
            Name = name;

            var request = HttpWebRequest.Create(absoluteUrl);
            var stream = request.GetResponse().GetResponseStream();
            var image = (Bitmap)Bitmap.FromStream(stream);

            Image = image;
        }

        public int Width { get { return Image.Width; } }
        public int Height { get { return Image.Height; } }
        public Bitmap Image { get; private set; }
        public string Name { get; private set; }
    }
}