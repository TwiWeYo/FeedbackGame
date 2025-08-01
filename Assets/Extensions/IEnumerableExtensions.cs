using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T1 MinBy<T1, T2>(this IEnumerable<T1> data, Func<T1, T2> selector)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var min = data.Min(selector);
            return data.First(q => selector.Invoke(q).Equals(min));
        }

        public static T1 MaxBy<T1, T2>(this IEnumerable<T1> data, Func<T1, T2> selector)
        {
            if (data is null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (selector is null)
            {
                throw new ArgumentNullException(nameof(selector));
            }

            var min = data.Max(selector);
            return data.First(q => selector.Invoke(q).Equals(min));
        }
    }
}
