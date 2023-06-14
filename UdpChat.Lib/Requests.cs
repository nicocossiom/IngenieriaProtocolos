namespace UdpChat.Lib.Authentication
{
    /// <summary>
    /// Request to register a user to the server. After a <see cref="Request"/> there is associated expected <see cref="Response"/>
    /// </summary>
    public class Request : ChatSendable
    {
        /// <summary>
        /// Sets or gets the username of the user
        /// </summary>
        /// <value> The username of the user </value>
        public string Username { get; set; }
        /// <summary>
        /// Sets or gets the password of the user
        /// </summary>
        /// <value> The password of the user </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the type of the request.
        /// </summary>
        /// <value>The type of the request. See <see cref="RequestType"/> for available request types.</value>
        public RequestType Type { get; set; }


        /// <summary>
        /// Represents the type of a request.
        /// </summary>
        public enum RequestType
        {
            /// <inheritdoc/>

            REGISTER,
            /// <inheritdoc/>
            LOGIN,
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Request"/> class with the specified username, password, and request type.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <param name="type">The type of the request.</param>
        public Request(string username, string password, RequestType type)
        {
            this.Username = username;
            this.Password = password;
            this.Type = type;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Request({Type},{Username},{Password},{Timestamp})";
        }
    }



    /// <summary>
    /// Response to a <see cref="Request"/>. It has a message and a state.
    /// </summary>
    public class Response : ChatSendable
    {
        /// <inheritdoc/>
        public string Message { get; set; }
        /// <inheritdoc/>
        public State ResponseState { get; set; }
        /// <summary>
        /// The possible states of a response.
        /// </summary>    
        public enum State
        {
            /// <inheritdoc/>
            REGISTER_SUCCESS,
            /// <inheritdoc/>
            LOGIN_SUCCESS,
            /// <inheritdoc/>

            ALREADY_REGISTERED,
            /// <inheritdoc/>
            ALREADY_LOGGED_IN,
            /// <inheritdoc/>
            LOGIN_FAILED,
            /// <inheritdoc/>
            ERROR
        }
        /// <inheritdoc/>


        public Response(State responseState, String message)
        {
            this.ResponseState = responseState;
            this.Message = message;
        }
        /// <inheritdoc/>

        public override string ToString()
        {
            return $"RegisterResponse: {ResponseState}";
        }

    }
}