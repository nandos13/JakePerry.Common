using System;

namespace JakePerry
{
    /// <summary>
    /// A response object representing the result of some arbitrary request.
    /// </summary>
    public readonly struct Response<T>
    {
        private readonly Response m_inner;
        private readonly T m_value;

        /// <inheritdoc cref="Response.Exception"/>
        public Exception Exception => m_inner.Exception;

        /// <inheritdoc cref="Response.Success"/>
        public bool Success => m_inner.Success;

        /// <summary>
        /// Typeless response with no value object.
        /// </summary>
        public Response Typeless => m_inner;

        /// <summary>
        /// The value associated with the successful response.
        /// <para/>
        /// A failed Response will always return <see langword="default"/>.
        /// </summary>
        public T Value => m_value;

        private Response(Response inner, T value)
        {
            m_inner = inner;
            m_value = value;
        }

        /// <summary>
        /// Construct a successful response containing the given <paramref name="value"/>.
        /// </summary>
        public Response(T value)
        {
            m_inner = Response.Successful();
            m_value = value;
        }

        /// <summary>
        /// Cast the value associated with this response object using the
        /// given <paramref name="selector"/> function.
        /// </summary>
        /// <returns>
        /// If the current response was successful, returns a new
        /// Response object containing the current value casted to
        /// type <typeparamref name="V"/>;
        /// Otherwise, returns a failed Response object and the
        /// <paramref name="selector"/> function is not invoked.
        /// </returns>
        /// <exception cref="ArgumentNullException"/>
        public Response<V> Cast<V>(Func<T, V> selector)
        {
            _ = selector ?? throw new ArgumentNullException(nameof(selector));
            if (m_inner.Success)
            {
                var v = selector.Invoke(m_value);
                return new Response<V>(v);
            }
            else
            {
                return new Response<V>(Response.Failed(), default);
            }
        }

        /// <inheritdoc cref="Response.Throw"/>
        public void Throw()
        {
            m_inner.Throw();
        }

        /// <inheritdoc cref="Response.Failed"/>
        public static Response<T> Failed()
        {
            return new Response<T>(Response.Failed(), default);
        }

        public static implicit operator Response(Response<T> typed)
        {
            return typed.Typeless;
        }

        public static implicit operator Response<T>(Response typeless)
        {
            return new Response<T>(typeless, default);
        }

        public static implicit operator Response<T>(Exception ex)
        {
            return (Response<T>)new Response(ex);
        }
    }
}
