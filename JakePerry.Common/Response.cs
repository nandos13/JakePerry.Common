using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace JakePerry
{
    /// <summary>
    /// A response object representing the result of some arbitrary request.
    /// </summary>
    public readonly struct Response
    {
        private readonly ExceptionDispatchInfo m_exception;
        private readonly bool m_success;

        internal ExceptionDispatchInfo ExceptionInfo => m_exception;

        /// <summary>
        /// The exception associated with the failure.
        /// <para/>
        /// A successful Response will never contain an exception.
        /// </summary>
        public Exception Exception => m_exception?.SourceException;

        /// <summary>
        /// Indicates whether the request succeeded.
        /// </summary>
        public bool Success => m_success;

        private Response(bool success, ExceptionDispatchInfo ex)
        {
            m_exception = ex;
            m_success = success;
        }

        internal Response(ExceptionDispatchInfo ex)
        {
            m_exception = ex;
            m_success = false;
        }

        /// <summary>
        /// Construct a failed response object containing an exception.
        /// <para/>
        /// Unthrown exceptions are supported.
        /// If the exception has no stack trace (ie. it has not been thrown),
        /// a stack trace will be generated in place.
        /// </summary>
        public Response(Exception ex)
        {
            if (ex is not null)
            {
                if (ex.StackTrace is null)
                {
                    try
                    {
                        throw ex;
                    }
                    catch (Exception) { }
                }

                m_exception = ExceptionDispatchInfo.Capture(ex);
            }
            else
            {
                m_exception = null;
            }
            m_success = false;
        }

        /// <summary>
        /// Throw the exception contained in this response, if there is one.
        /// </summary>
        public void Throw()
        {
            m_exception?.Throw();
        }

        /// <summary>
        /// Check if a <paramref name="token"/> is canceled and create a failure response
        /// containing an <see cref="OperationCanceledException"/> if so.
        /// </summary>
        /// <returns>
        /// Returns <see langword="true"/> if cancellation was requested and execution
        /// should be aborted; Otherwise, returns <see langword="false"/>.
        /// </returns>
        public static bool CheckCanceled(CancellationToken token, out Response canceledResponse)
        {
            try
            {
                token.ThrowIfCancellationRequested();
            }
            catch (OperationCanceledException ex)
            {
                canceledResponse = new Response(ex);
                return true;
            }

            canceledResponse = default;
            return false;
        }

        /// <summary>
        /// Construct a failed response object with no exception.
        /// </summary>
        public static Response Failed() => new Response(false, null);

        /// <summary>
        /// Construct a successful response object.
        /// </summary>
        public static Response Successful() => new Response(true, null);

        public static implicit operator Response(bool value)
        {
            return value ? Successful() : Failed();
        }

        public static implicit operator Response(Exception ex)
        {
            return new Response(ex);
        }
    }
}
