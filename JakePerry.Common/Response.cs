using System;
using System.Diagnostics;
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
        private readonly StackTrace m_failureStackTrace;

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
            m_failureStackTrace = null;

            if (!success && ex is null)
            {
                m_failureStackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
            }
        }

        internal Response(ExceptionDispatchInfo ex)
        {
            m_exception = ex;
            m_success = false;
            m_failureStackTrace = null;

            if (ex is null)
            {
                m_failureStackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
            }
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
                    ExceptionUtility.SetStackTrace(ex, Environment.StackTrace);
                }

                m_exception = ExceptionDispatchInfo.Capture(ex);
                m_failureStackTrace = null;
            }
            else
            {
                m_exception = null;
                m_failureStackTrace = new StackTrace(skipFrames: 1, fNeedFileInfo: true);
            }
            m_success = false;
        }

        /// <summary>
        /// Log the exception contained in this response, if there is one.
        /// <para/>
        /// If the current Response object describes a failure without a known
        /// exception and <paramref name="logUnknown"/> is <see langword="true"/>,
        /// an error is logged. If a stack trace was captured when the response was
        /// constructed, the stack trace will be sent with the log.
        /// </summary>
        /// <param name="logUnknown">
        /// A flag indicating whether the call should log an error if this is an unknown failure.
        /// </param>
        public void LogFailure(bool logUnknown)
        {
            var ex = Exception;

            if (ex is null && logUnknown)
            {
                if (m_failureStackTrace is not null)
                {
                    JPDebug.LogError(m_failureStackTrace, "An unknown failure occurred.");
                }
                else
                {
                    JPDebug.LogError(
                        "An unknown failure occurred. No stack trace was captured at the time of failure. " +
                        "Use the Response.Failed() method rather than the default instance to capture the trace.");
                }
            }

            if (ex is not null)
            {
                JPDebug.LogException(ex);
            }
        }

        /// <summary>
        /// Throw the exception contained in this response, if there is one.
        /// If the current Response object describes a failure without a known
        /// exception and <paramref name="throwUnknown"/> is <see langword="true"/>,
        /// an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="throwUnknown">
        /// A flag indicating whether the call should throw if this is an unknown failure.
        /// </param>
        public void Throw(bool throwUnknown)
        {
            m_exception?.Throw();
            if (throwUnknown)
            {
                throw new InvalidOperationException("An unknown failure occurred.");
            }
        }

        /// <summary>
        /// Report the failure described by the current Response object, if it was not successful.
        /// <para/>
        /// <see cref="ErrorHandlingPolicy.Log"/> reports the failure via
        /// <see cref="LogFailure(bool)"/>, and <see cref="ErrorHandlingPolicy.Throw"/>
        /// via <see cref="Throw(bool)"/>.
        /// For each method, the <paramref name="reportUnknown"/> flag is passed to indicate whether
        /// an unknown failure should be reported or ignored.
        /// </summary>
        /// <param name="o">
        /// Policy describing how to report the failure.
        /// </param>
        /// <param name="reportUnknown">
        /// A flag indicating whether the response should be reported if it represents an unknown failure.
        /// </param>
        public void ReportFailure(ErrorHandlingPolicy o, bool reportUnknown)
        {
            switch (o)
            {
                case ErrorHandlingPolicy.Throw: Throw(reportUnknown); break;
                case ErrorHandlingPolicy.Log: LogFailure(reportUnknown); break;
            }
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
