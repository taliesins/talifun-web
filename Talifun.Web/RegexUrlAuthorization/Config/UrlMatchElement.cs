using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Principal;
using System.Web.Configuration;
using Talifun.Web.Configuration;

namespace Talifun.Web.RegexUrlAuthorization.Config
{
    public sealed class UrlMatchElement : NamedConfigurationElement
    {
        private static ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty name = new ConfigurationProperty("name", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty expression = new ConfigurationProperty("expression", typeof(string), null, ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty rules = new ConfigurationProperty(null, typeof(AuthorizationRuleCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        private bool everyoneAllowed;

        /// <summary>
        /// Initializes the <see cref="UrlMatchElement"/> class.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static UrlMatchElement()
        {
            properties.Add(name);
            properties.Add(expression);
            properties.Add(rules);
        }

        /// <summary>
        /// Gets or sets the name of the configuration element represented by this instance.
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = null, IsRequired = true, IsKey = true)]
        public override string Name
        {
            get { return ((string)base[name]); }
            set { base[name] = value; }
        }

        /// <summary>
        /// Gets or sets the expression to use for matching against the url.
        /// </summary>
        [ConfigurationProperty("expression", DefaultValue = null, IsRequired = true)]
        public string Expression
        {
            get { return ((string)base[expression]); }
            set { base[expression] = value; }
        }

        /// <summary>
        /// Gets the collection of configuration properties contained by this configuration element.
        /// </summary>
        /// <returns>The <see cref="T:System.Configuration.ConfigurationPropertyCollection"></see> collection of properties for the element.</returns>
        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return properties;
            }
        }

        private static MethodInfo isUserAllowedMethod = typeof(AuthorizationRuleCollection).GetMethod("IsUserAllowed", BindingFlags.Instance | BindingFlags.NonPublic);
        internal bool IsUserAllowed(IPrincipal user, string verb)
        {
            return (bool)isUserAllowedMethod.Invoke(this.Rules, new object[] { user, verb });
        }


        private static PropertyInfo everyoneAllowedProperty = typeof(AuthorizationRule).GetProperty("Everyone", BindingFlags.Instance | BindingFlags.NonPublic);
        protected override void PostDeserialize()
        {
            if (this.Rules.Count > 0)
            {
                var rule = this.Rules[0];
                this.everyoneAllowed = (rule.Action == AuthorizationRuleAction.Allow) && (bool)everyoneAllowedProperty.GetValue(rule, null);
            }
        }

        internal bool EveryoneAllowed
        {
            get
            {
                return this.everyoneAllowed;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AuthorizationRuleCollection Rules
        {
            get
            {
                return (AuthorizationRuleCollection)base[rules];
            }
        }
    }
}
