using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace Talifun.Web.Configuration
{
    /// <summary>
    /// Defines a generic collection class for strongly-typed configuration elements, where <typeparamref name="T"/> is a type derived from <see cref="System.Configuration.ConfigurationElement" />.
    /// </summary>
    /// <typeparam name="T">A type derived from <see cref="System.Configuration.ConfigurationElement" />.</typeparam>
    public class CurrentConfigurationElementCollection<T> : ConfigurationElementCollection where T : ConfigurationElement, new()
    {
        /// <summary>
        /// The collection of instances of classes derived from <see cref="ConfigurationElement" />  that represent the configuration properties containing in this collection.
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "Visibility is limted to derived classes which should close the generic type and treat the field as private")]
        [SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification = "Visibility is limted to derived classes which should close the generic type and treat the field as private")]
        protected static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        /// <summary>
        /// Gets the collection of configuration properties contained by this configuration element collection.
        /// </summary>
        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        /// <summary>
        /// Gets the <see cref="ConfigurationElementCollectionType" /> of the <see cref="T:System.Configuration.ConfigurationElementCollection" />.
        /// </summary>
        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        /// <summary>
        /// Gets or sets the configuration element at the specified index.
        /// </summary>
        public T this[int index]
        {
            get { return base.BaseGet(index) as T; }
            set
            {
                if (base.BaseGet(index) != null) base.BaseRemoveAt(index);
                this.BaseAdd(index, value);
            }
        }

        /// <summary>
        /// Gets or sets the configuration element with the specified name.
        /// </summary>
        new public T this[string name]
        {
            get { return base.BaseGet(name) as T; }
            set
            {
                if (base.BaseGet(name) != null) base.BaseRemove(name);
                this.BaseAdd(value);
            }
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <returns>
        /// A new <typeparamref name="T" />.
        /// </returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Configuration.ConfigurationElement" /> to return the key for.</param>
        /// <returns>
        /// An <see cref="T:System.Object"></see> that acts as the key for the specified <see cref="T:System.Configuration.ConfigurationElement"></see>.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Thrown when the specified <see cref="ConfigurationElement" /> does not expose <c>Name</c> as a property.</exception>
        protected override object GetElementKey(ConfigurationElement element)
        {
            var namedElement = element as NamedConfigurationElement;
            if (null != namedElement) return (namedElement.Name);
            throw (new InvalidOperationException("ConfigurationElement does not expose Name as a property"));
        }
    }
}
