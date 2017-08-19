using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace Gluon
{
    public class ClassesStructureCreator
    {
        #region Private Members

        public const string ReturnVal = "returnval";
        public const string HelpersFileName = "_Helpers.cs";
        public const string DefaultHelperNamespace = "Gluon";
        public const string CompileProjectItemType = "Compile";
        public const string FolderProjectItemType = "Folder";
        public const string SelfDescClassAttrClause = "[SelfDescriptedClass]";
        public const string FullSelfDescClassAttrClause = "[FullSelfDescriptedClass]";


        private OracleDbConnection _oracleDbConnection;

        /// <summary>
        ///     Config section for common settings
        /// </summary>
        private CommonSettings _commonConfigSection;

        /// <summary>
        ///     Сonfig section for working with TFS
        /// </summary>
        private TfsSettings _tfsConfigSection;

        /// <summary>
        ///     The destination project name, where will be created folder with generated files
        /// </summary>
        private readonly string _targetProjectPath;

        /// <summary>
        ///     Folder in destination project where generated files will be copied 
        /// </summary>
        private readonly string _destinatonFolderPath;

        /// <summary>
        ///     Convertin indentation level to сorresponding number of spaces
        /// </summary>
        /// <param name="indentLevel"> Indentation level (from 1) </param>
        /// <returns> Spaces </returns>
        private string IndentLevelToStr(int indentLevel)
        {
            var indentCount = _commonConfigSection.IndentValue * indentLevel;
            var formatClause = String.Format("{{0, {0}}}", indentCount);
            var indent = String.Format(formatClause, " ");
            return indent;
        }

        /// <summary>
        ///     Creating of the list of strings describing class with properties
        /// </summary>
        /// <param name="attributeClause"> [FullSelfDescriptedClass] or [SelfDescriptedClass]</param>
        /// <param name="classes"> Dictionary [classname, [properties]]</param>
        /// <param name="indentLevel"> Class nesting level </param>
        /// <param name="closingBrace"> Closing brase (false for class containing inner classes) </param>
        /// <returns> List of strings - class body </returns>
        private List<string> CreateClasses(string attributeClause, Dictionary<string, List<string>> classes,
            int indentLevel, bool closingBrace = true)
        {
            var body = new List<string>();
            foreach (var _class in classes)
            {
                var header = new List<string>
                {
                    "",
                    IndentLevelToStr(indentLevel) + attributeClause,
                    IndentLevelToStr(indentLevel) + String.Format("public class {0}", _class.Key.ToUpper()),
                    IndentLevelToStr(indentLevel) + "{",
                    IndentLevelToStr(indentLevel + 1) +
                    String.Format("static {0}() {{ Initializer.Init<{0}>(); }}", _class.Key.ToUpper())
                };

                var classBody =
                    _class.Value
                        .Where(s => s != ReturnVal)
                        .Select(
                            s =>
                                IndentLevelToStr(indentLevel + 1) +
                                String.Format("public static string {0} {{ get; set; }}", s.ToCapital()))
                        .ToList();
                var retunClause = _class.Value.FirstOrDefault(s => s == ReturnVal);
                if (retunClause != null)
                {
                    classBody.Add(IndentLevelToStr(indentLevel + 1) +
                                  String.Format("public static string {0} {{ get {{ return \"return\"; }} }}",
                                      _commonConfigSection.StoredProcReturnName));
                }
                if (closingBrace)
                    classBody.Add(IndentLevelToStr(indentLevel) + "}");
                body.AddRange(header);
                body.AddRange(classBody);
            }
            return body;
        }

        /// <summary>
        ///     Creating of the cs-file, consisted of class, describing stored procedures
        /// </summary>
        /// <param name="packageName"> Oracle package </param>
        /// <param name="procedures"> Dictionary [storedProc [params]]</param>
        /// <returns> Generated file for including in destination project </returns>
        private bool CreateFile(string packageName, Dictionary<string, List<string>> procedures)
        {
            var body = new List<string>
            {
                String.Format("namespace {0}.{1}", _commonConfigSection.TargetProjectNameSpace, _commonConfigSection.Namespace),
                "{"
            };

            var packageClass = CreateClasses(FullSelfDescClassAttrClause,
                new Dictionary<string, List<string>> {{packageName, procedures.Keys.ToList()}}, 1, false);
            var proceduresClasses = CreateClasses(FullSelfDescClassAttrClause, procedures, 2);
            body.AddRange(packageClass);
            body.AddRange(proceduresClasses);
            body.Add(IndentLevelToStr(1) + "}");
            body.Add("}");
            var fullFileName = String.Format("{0}\\{1}.cs", _destinatonFolderPath, packageName);
            var fileBody = String.Join(Environment.NewLine, body);

            var isTheSameFile = File.Exists(fullFileName) && File.ReadAllText(fullFileName) == fileBody;
            if (!isTheSameFile)
                File.WriteAllText(fullFileName, fileBody);
            return isTheSameFile;
        }

        /// <summary>
        ///     Getting of the solution project for including folder and files
        /// </summary>
        /// <returns> Solution project </returns>
        private Project GetProject()
        {
            var fullProjectName = String.Format("{0}\\{1}.csproj", _targetProjectPath, _commonConfigSection.TargetProject);
            var project =
                ProjectCollection.GlobalProjectCollection
                    .LoadedProjects.FirstOrDefault(pr => pr.FullPath == fullProjectName) ??
                new Project(fullProjectName);
            return project;
        }

        /// <summary>
        ///     Safe adding of the file to project
        /// </summary>
        /// <param name="project"> Target project </param>
        /// <param name="fileName"> Name of file to including to project </param>
        /// <returns> true - file already exists in project </returns>
        private bool AddingFileToProject(Project project, string fileName)
        {
            var fileNameWithNamespace = String.Format("{0}\\{1}.cs", _commonConfigSection.Namespace, fileName);
            var itemAlreadyExists = project.Items.Any(i => i.EvaluatedInclude == fileNameWithNamespace);
            if (!itemAlreadyExists)
                project.AddItem(CompileProjectItemType, fileNameWithNamespace);
            return itemAlreadyExists;
        }

        /// <summary>
        ///     Adding of the class-helper to destination project
        /// </summary>
        /// <param name="project"> Solution project </param>
        /// <returns> true - need to save project </returns>
        private bool CreateAndAddHelperToProject(Project project)
        {
            var helperBody = Utils.GetFileBody(HelpersFileName);
            // The replacing template of namespace to namespace specified in config
            helperBody = helperBody.Replace(DefaultHelperNamespace,
                String.Format("{0}.{1}", _commonConfigSection.TargetProjectNameSpace, _commonConfigSection.Namespace));
            // Replacing substring "Return" in body of _Helpers.cs on name specified in config
            helperBody = helperBody.Replace("Return", _commonConfigSection.StoredProcReturnName);
            var helperFullFileName = Path.Combine(_destinatonFolderPath, HelpersFileName);
            var isTheSameFile = File.Exists(helperFullFileName) && File.ReadAllText(helperFullFileName) == helperBody;
            if (!isTheSameFile)
                File.WriteAllText(helperFullFileName, helperBody);
            var needToAddFileToProject =
                !AddingFileToProject(project, Path.GetFileNameWithoutExtension(HelpersFileName));
            var result = !isTheSameFile || needToAddFileToProject;
            return result;
        }

        /// <summary>
        ///     Adding of the folder and its files in a source control
        /// </summary>
        /// <param name="destinationProjectPath"> Path to the destination project </param>
        /// <param name="targetFolder"> Target folder in destination project </param>
        public void AddingToVersionControl(string destinationProjectPath, string targetFolder)
        {
            var solutionPath = Path.GetDirectoryName(destinationProjectPath);
            var teamProjectCollection =
                TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(_tfsConfigSection.Uri));
            var versionControlServer = teamProjectCollection.GetService<VersionControlServer>();
            var workspace = versionControlServer.GetWorkspace(solutionPath);
            var versionControlFolderPath = String.Format("{0}/{1}/{2}", _tfsConfigSection.ProjectPath, _commonConfigSection.TargetProject, targetFolder);
            var targetFolderPath = Path.Combine(destinationProjectPath, targetFolder);

            var oracleHelpersItems = versionControlServer.GetItems(versionControlFolderPath);
            var folderExists = oracleHelpersItems.Items.Any();
            if (!folderExists)
            {
                workspace.PendAdd(targetFolderPath, true);
            }
            else
            {
                workspace.PendEdit(targetFolderPath, RecursionType.OneLevel);
            }
        }

        /// <summary>
        ///     Getting sections of app.config
        /// </summary>
        private void GettingAppConfigSections()
        {
            _commonConfigSection = (CommonSettings)ConfigurationManager.GetSection("CommonSettings");
            if (_commonConfigSection.UsingTfs)
                _tfsConfigSection = (TfsSettings)ConfigurationManager.GetSection("TfsSettings");
        }

        #endregion

        #region Public Members

        /// <summary>
        ///     Constructor
        /// </summary>
        public ClassesStructureCreator()
        {
            GettingAppConfigSections();
            // Current project directory info
            var currentProjectDirectoryInfo = Directory.GetParent(Directory.GetCurrentDirectory()).Parent;
            if (currentProjectDirectoryInfo != null && currentProjectDirectoryInfo.Parent != null)
            {
                _targetProjectPath = Path.Combine(
                    // Here should be solution project directory, if not - correct the getting one
                    currentProjectDirectoryInfo.Parent.FullName,                    
                    _commonConfigSection.TargetProject);                
                _destinatonFolderPath = Path.Combine(_targetProjectPath, _commonConfigSection.Namespace);
                Directory.CreateDirectory(_destinatonFolderPath);
            }
        }

        /// <summary>
        ///     Main method. Creating a file structure containing classes corresponding to oracle packages
        /// </summary>
        public void CreateStructure()
        {
            _oracleDbConnection = new OracleDbConnection();
            var packages = _oracleDbConnection.GetData();
            var project = GetProject();
            project.AddItem(FolderProjectItemType, _destinatonFolderPath);

            var needToSaveProject = CreateAndAddHelperToProject(project);

            foreach (var package in packages)
            {
                needToSaveProject |= !CreateFile(package.Key, package.Value);

                needToSaveProject |= !AddingFileToProject(project, package.Key);
            }
            if (needToSaveProject)
            {
                if (_commonConfigSection.UsingTfs)
                    AddingToVersionControl(_targetProjectPath, _commonConfigSection.Namespace);
                project.Save();
            }
        }

        #endregion
    }
}