using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BringYourOwnAI.Core.Interfaces;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace BringYourOwnAI.VsIntegration
{
    public class VsSolutionService : IVsSolutionService
    {
        private DTE2 _dte = null!;
        private readonly IServiceProvider _serviceProvider;

        public VsSolutionService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private DTE2 GetDte()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return _dte ??= (_serviceProvider.GetService(typeof(DTE)) as DTE2)!;
        }

        public async Task<IEnumerable<string>> GetSolutionFilesAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var files = new List<string>();
            var dte = GetDte();
            if (dte.Solution == null) return files;

            foreach (Project project in dte.Solution.Projects)
            {
                files.AddRange(GetProjectFiles(project));
            }

            return files;
        }

        private IEnumerable<string> GetProjectFiles(Project project)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var files = new List<string>();
            if (project.ProjectItems == null) return files;

            foreach (ProjectItem item in project.ProjectItems)
            {
                files.AddRange(GetProjectItemFiles(item));
            }
            return files;
        }

        private IEnumerable<string> GetProjectItemFiles(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var files = new List<string>();
            
            // FileNames is 1-indexed and usually index 1 is the physical path
            try { files.Add(item.FileNames[1]); } catch { }

            if (item.ProjectItems != null)
            {
                foreach (ProjectItem subItem in item.ProjectItems)
                {
                    files.AddRange(GetProjectItemFiles(subItem));
                }
            }
            return files;
        }

        public async Task<string> ReadFileAsync(string path)
        {
            // If the file is open in VS, we might want to get the latest buffer
            // For now, just standard IO, but could be enhanced with IVsRunningDocumentTable
            return await Task.Run(() => File.ReadAllText(path));
        }

        public async Task WriteFileAsync(string path, string content)
        {
            await Task.Run(() => File.WriteAllText(path, content));
            
            // Notify VS that the file changed or try to refresh Solution Explorer
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = GetDte();
            dte.ItemOperations.OpenFile(path); // Simple way to ensure it's loaded/refreshed
        }

        public async Task<string> GetActiveDocumentContextAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = GetDte();
            if (dte.ActiveDocument == null) return string.Empty;
            
            return dte.ActiveDocument.FullName;
        }

        public async Task RunBuildAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var dte = GetDte();
            dte.Solution.SolutionBuild.Build(true);
        }
    }
}
