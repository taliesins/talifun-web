using System;

namespace Talifun.Web.Compress
{
    public class Quadruplet<TFirst, TSecond, TThird, TFourth>
    {
        public Quadruplet(TFirst first, TSecond second, TThird third, TFourth forth)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            if (second == null)
            {
                throw new ArgumentNullException("third");
            }
            if (second == null)
            {
                throw new ArgumentNullException("forth");
            }
            First = first;
            Second = second;
            Third = third;
            Forth = forth;
        }

        public virtual TFirst First { get; private set; }
        public virtual TSecond Second { get; private set; }
        public virtual TThird Third { get; private set; }
        public virtual TFourth Forth { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            var other = obj as Quadruplet<TFirst, TSecond, TThird, TFourth>;
            return (other != null) && (other.First.Equals(First)) && (other.Second.Equals(Second)) && (other.Third.Equals(Third)) && (other.Forth.Equals(Forth));
        }

        public override int GetHashCode()
        {
            var a = First.GetHashCode();
            var c = Third.GetHashCode();

            var ab = ((a << 5) + a) ^ Second.GetHashCode();
            var cd = ((c << 5) + a) ^ Third.GetHashCode();

            return ((ab << 5) + ab) ^ cd.GetHashCode();
        }
    }
}