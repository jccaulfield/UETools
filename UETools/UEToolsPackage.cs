﻿using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;
using Task = System.Threading.Tasks.Task;

namespace UETools
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(UEToolsPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class UEToolsPackage : AsyncPackage
    {
        /// <summary>
        /// UEToolsPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "fd6e66a6-d800-4ab4-881d-369473eea1cb";

        /// <summary>
        /// Initializes a new instance of the <see cref="UEToolsPackage"/> class.
        /// </summary>
        public UEToolsPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            //var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\VisualStudio\" + (Package.GetGlobalService(typeof(EnvDTE.DTE)) as EnvDTE80.DTE2).Version + @"\General");
            //key.SetValue("EnableVSIPLogging", 1);

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            var commandSetGuid = new Guid("a4eb01f6-f054-4d41-9571-e6e38166d654");
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0100, (o, e) => Helper.P4Helper.ExecuteP4Command("edit {0}", Helper.VSHelper.GetOpenDocumentName()));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0101, (o, e) => Helper.P4Helper.ExecuteP4Command("revert {0}", Helper.VSHelper.GetOpenDocumentName()));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0102, (o, e) => Helper.P4Helper.ExecuteP4VCommand("history {0}", Helper.VSHelper.GetOpenDocumentName()));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0103, (o, e) => Helper.P4Helper.ExecuteP4VCommand("timelapse {0}", Helper.VSHelper.GetOpenDocumentName()));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0104, (o, e) => Helper.P4Helper.ExecuteP4VCommand("prevdiff {0}", Helper.VSHelper.GetOpenDocumentName()));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0105, (o, e) => Helper.P4Helper.ExecuteP4VCCommand("revisiongraph {0}", Helper.VSHelper.GetOpenDocumentName()));

            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0150, (o, e) => Helper.VSHelper.ForEachSelectedFile(filename => Helper.P4Helper.ExecuteP4Command("edit {0}", filename)));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0151, (o, e) => Helper.VSHelper.ForEachSelectedFile(filename => Helper.P4Helper.ExecuteP4Command("revert {0}", filename)));
            await VisualStudio.CommandAction.InitializeAsync(this, commandSetGuid, 0x0152, (o, e) => Helper.VSHelper.ForEachSelectedFile(filename => Helper.P4Helper.ExecuteP4VCommand("prevdiff {0}", filename)));

            // Add handlers so that we can rename the toolbar
            var dte = (EnvDTE80.DTE2)GetGlobalService(typeof(EnvDTE.DTE));
            dte.Events.SolutionEvents.AfterClosing += OnIdeSolutionEvent;
            dte.Events.SolutionEvents.Opened += OnIdeSolutionEvent;
            dte.Events.SolutionEvents.Renamed += OnIdeSolutionEvent;
        }

        private void OnIdeSolutionEvent(string oldname)
        {
            UpdateWindowTitle();
        }

        private void OnIdeSolutionEvent()
        {
            UpdateWindowTitle();
        }

        private void UpdateWindowTitle()
        {
            var dte = (EnvDTE80.DTE2)GetGlobalService(typeof(EnvDTE.DTE));
            if (dte.MainWindow != null)
            {
                //System.Windows.Application.Current.MainWindow.Title = "Testing!";
                //System.Windows.Forms.Application.
            }
        }

        #endregion
    }
}