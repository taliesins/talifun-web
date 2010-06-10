namespace Talifun.Web
{
    public enum HttpMethod
    {
        /// <summary>
        /// This specification reserves the method name CONNECT for use with a proxy that can dynamically switch to being a tunnel (e.g. SSL tunneling). 
        /// </summary>
        Connect = 7,
        /// <summary>
        /// The DELETE method requests that the origin server delete the resource identified by the Request-URI. 
        /// </summary>
        Delete = 4,
        /// <summary>
        /// The GET method means retrieve whatever information (in the form of an entity) is identified by the Request-URI. If the Request-URI refers to a data-producing process, it is the produced data which shall be returned as the entity in the response and not the source text of the process, unless that text happens to be the output of the process. 
        /// </summary>
        Get = 1,
        /// <summary>
        /// response to a HEAD request SHOULD be identical to the information sent in response to a GET request. This method can be used for obtaining metainformation about the entity implied by the request without transferring the entity-body itself. This method is often used for testing hypertext links for validity, accessibility, and recent 
        /// </summary>
        Head = 0,
        Options = 5,
        Other = -1,
        /// <summary>
        /// The POST method is used to request that the origin server accept the entity enclosed in the request as a new subordinate of the resource identified by the Request-URI in the Request-Line. 
        /// </summary>
        Post = 2,
        /// <summary>
        /// The PUT method requests that the enclosed entity be stored under the supplied Request-URI. 
        /// </summary>
        Put = 3,
        /// <summary>
        /// The TRACE method is used to invoke a remote, application-layer loop- back of the request message. 
        /// </summary>
        Trace = 6
    }
}
