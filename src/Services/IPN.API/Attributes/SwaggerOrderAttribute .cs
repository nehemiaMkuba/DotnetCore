using System;

namespace IPN.API.Attributes
{
    /// <summary>
    /// Annotates a controller with a Swagger sorting order that is used when generating the Swagger documentation to
    /// order the controllers in a specific desired order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class SwaggerOrderAttribute : System.Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerOrderAttribute"/> class.
        /// </summary>
        /// <param name="order">Sets the sorting order of the controller.</param>
        public SwaggerOrderAttribute(string order)
        {
            Order = order;
        }

        /// <summary>
        /// Gets the sorting order of the controller.
        /// </summary>
        public string Order { get; private set; }
    }
}
