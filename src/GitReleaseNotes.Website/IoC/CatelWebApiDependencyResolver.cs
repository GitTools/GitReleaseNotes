namespace GitReleaseNotes.Website.IoC
{
    using Catel;
    using Catel.IoC;

    public class CatelWebApiDependencyResolver : Catel.IoC.DependencyResolver, System.Web.Http.Dependencies.IDependencyResolver
    {
        private readonly IServiceLocator _serviceLocator;
        private readonly ITypeFactory _typeFactory;

        public CatelWebApiDependencyResolver()
            : this(ServiceLocator.Default)
        {
        }

        public CatelWebApiDependencyResolver(IServiceLocator serviceLocator)
        {
            Argument.IsNotNull(() => serviceLocator);

            _serviceLocator = serviceLocator;
            _typeFactory = serviceLocator.ResolveType<ITypeFactory>();
        }

        #region IDependencyResolver Members
        public System.Web.Http.Dependencies.IDependencyScope BeginScope()
        {
            // This resolver does not support child scopes, so we simply return 'this'.
            return this;
        }

        public void Dispose()
        {
            // nothing to dispose
        }
        #endregion
    }
}