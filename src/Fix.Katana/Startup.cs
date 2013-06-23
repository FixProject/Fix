namespace Fix.Katana
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Owin;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    /// <summary>
    /// Startup class for discovery by katana.exe
    /// </summary>
    public class Startup
    {
        private static readonly object SyncApp = new object();
        private AppFunc _app;

        /// <summary>
        /// Used by katana engine to set up middlewares
        /// </summary>
        public void Configuration(IAppBuilder app)
        {
            this.BuildApp();

            app.Use(new Func<AppFunc, AppFunc>(ignoreNextApp => (AppFunc)this.Invoke));
        }

        /// <summary>
        /// This is only here for discovery by katana running independently.
        /// </summary>
        public Task Invoke(IDictionary<string, object> environment)
        {
            return this._app(environment);
        }

        /// <summary>
        /// If we are using katana from command-line w/discovery we need to hookup, so
        /// we can't leave this to custom katana engine usage. 
        /// </summary>
        private void BuildApp(string appPath = null)
        {
            lock (SyncApp)
            {
                if (_app == null)
                {
                    if (string.IsNullOrWhiteSpace(appPath))
                    {
                        appPath =
                            Path.GetDirectoryName(AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName.StartsWith("Simple.Web")).Location)
                            ?? AppDomain.CurrentDomain.BaseDirectory;
                    }

                    var fixer = new Fixer();

                    var aggregateCatalog = new AggregateCatalog();
                    foreach (var file in Directory.EnumerateFiles(appPath, "*.dll"))
                    {
                        var justFileName = Path.GetFileName(file);

                        if (justFileName == null)
                        {
                            continue;
                        }

                        if (justFileName.StartsWith("Microsoft.") || justFileName.StartsWith("System."))
                        {
                            continue;
                        }

                        var catalog = new AssemblyCatalog(file);
                        aggregateCatalog.Catalogs.Add(catalog);
                    }

                    var container = new CompositionContainer(aggregateCatalog);
                    container.ComposeParts(fixer);

                    this._app = fixer.BuildApp();
                }
            }
        }
    }
}