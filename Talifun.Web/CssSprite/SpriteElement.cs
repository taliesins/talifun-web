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
            Image = (Bitmap)Bitmap.FromStream(stream);
            BorderWidth = 2;
            Rectangle = new Rectangle(Point.Empty, new Size(Image.Width + BorderWidth*2, Image.Height + BorderWidth*2));

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
            BorderWidth = 2;
            Rectangle = new Rectangle(Point.Empty, new Size(Image.Width + BorderWidth*2, Image.Height + BorderWidth*2));
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
            Image = (Bitmap)Bitmap.FromStream(stream);
            BorderWidth = 2;
            Rectangle = new Rectangle(Point.Empty, new Size(Image.Width + BorderWidth*2, Image.Height + BorderWidth*2));
        }

        public int BorderWidth { get; private set; }
        public Rectangle Rectangle { get; set; }
        public Bitmap Image { get; private set; }
        public string Name { get; private set; }
    }
}