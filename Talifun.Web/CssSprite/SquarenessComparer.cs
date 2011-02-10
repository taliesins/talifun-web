using System.Collections.Generic;

namespace Talifun.Web.CssSprite
{
    public class SquarenessComparer : IComparer<SpriteElement> 
    {
        #region IComparer<SpriteElement> Members

        // Return -1, 0, or 1 to indicate whether
        // x belongs before, the same as, or after y.
        // Sort by squareness, area, height, width descending.
        public int Compare(SpriteElement x, SpriteElement y)
        {
            var xsq = System.Math.Abs(x.Rectangle.Width - x.Rectangle.Height);
            var ysq = System.Math.Abs(y.Rectangle.Width - y.Rectangle.Height);
            if (xsq < ysq) return -1;
            if (xsq > ysq) return 1;
            var xarea = x.Rectangle.Width * x.Rectangle.Height;
            var yarea = y.Rectangle.Width * y.Rectangle.Height;
            if (xarea < yarea) return 1;
            if (xarea > yarea) return -1;
            if (x.Rectangle.Height < y.Rectangle.Height) return 1;
            if (x.Rectangle.Height > y.Rectangle.Height) return -1;
            if (x.Rectangle.Width < y.Rectangle.Width) return 1;
            if (x.Rectangle.Width > y.Rectangle.Width) return -1;
            return 0;
        }

        #endregion
    }
}