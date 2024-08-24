using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;

namespace JakePerry
{
    internal static class DebugUtil
    {
        private static bool TryGetCount<T>(IEnumerable<T> collection, out int count)
        {
            if (collection is IReadOnlyCollection<T> c0) { count = c0.Count; return true; }
            if (collection is ICollection<T> c1) { count = c1.Count; return true; }
            if (collection is ICollection c2) { count = c2.Count; return true; }
            count = -1;
            return false;
        }

        internal static string StringifyCollection<T>(
            IEnumerable<T> collection,
            bool printCollectionType = true,
            bool printMetadata = true,
            string separator = ", ",
            string nullItemIdentifier = "<NULL>")
        {
            if (collection is null)
            {
                return "<NULL COLLECTION>";
            }

            var sb = new StringBuilder();

            if (printCollectionType)
            {
                sb.Append(collection.GetType().Name);
                sb.Append(' ');
            }

            int countIdx = -1;
            if (printMetadata)
            {
                sb.Append("(Count: ");

                if (TryGetCount(collection, out int count))
                {
                    sb.Append(count);
                }
                else
                {
                    countIdx = sb.Length;
                }
                sb.Append(')');
            }

            sb.Append("{ ");

            bool flag = false;
            int itemCount = 0;
            foreach (var item in collection)
            {
                ++itemCount;

                if (flag) sb.Append(separator);
                flag = true;

                if (item is null)
                {
                    sb.Append(nullItemIdentifier);
                }
                else
                {
                    sb.Append(item.ToString());
                }
            }

            sb.Append(" }");

            if (countIdx > -1)
            {
                sb.Insert(countIdx, itemCount);
            }

            return sb.ToString();
        }

        internal static void ReportException(Exception ex, ErrorHandlingPolicy policy)
        {
            if (ex is null) return;

            switch (policy)
            {
                case ErrorHandlingPolicy.Throw:
                    {
                        if (ex.StackTrace is null)
                        {
                            throw ex;
                        }
                        else
                        {
                            ExceptionDispatchInfo.Capture(ex).Throw();
                        }
                        break;
                    }
                case ErrorHandlingPolicy.Log:
                    {
                        JPDebug.LogException(ex);
                        break;
                    }
            }
        }
    }
}
