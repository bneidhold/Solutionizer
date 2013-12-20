﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using GongSolutions.Wpf.DragDrop;
using Solutionizer.Extensions;
using Solutionizer.Framework;
using Solutionizer.Models;
using Solutionizer.Services;

namespace Solutionizer.ViewModels {
    public class ProjectRepositoryViewModel : PropertyChangedBase, IDragSource {
        public delegate ProjectRepositoryViewModel Factory(ICommand doubleClickCommand);

        private readonly ISettings _settings;
        private string _rootPath;
        private ProjectFolder _rootFolder;
        private IList _nodes;
        private string _filter;
        private readonly ICommand _doubleClickCommand;
        private readonly ObservableCollection<ItemViewModel> _selectedItems = new ObservableCollection<ItemViewModel>();

        public ProjectRepositoryViewModel(ISettings settings, ICommand doubleClickCommand) {
            _settings = settings;
            _doubleClickCommand = doubleClickCommand;

            _settings.PropertyChanged += (sender, args) => {
                if (args.PropertyName == "ShowProjectCount") {
                    NotifyOfPropertyChange(() => ShowProjectCount);
                }
            };
        }

        public string RootPath {
            get { return _rootPath; }
            set {
                if (_rootPath != value) {
                    _rootPath = value;
                    NotifyOfPropertyChange(() => RootPath);
                }
            }
        }

        public ICommand DoubleClickCommand {
            get { return _doubleClickCommand; }
        }

        public ObservableCollection<ItemViewModel> SelectedItems {
            get { return _selectedItems; }
        }

        public ProjectFolder RootFolder {
            get { return _rootFolder; }
            set {
                if (_rootFolder != value) {
                    _rootFolder = value;
                    NotifyOfPropertyChange(() => RootFolder);
                    Nodes = CreateDirectoryViewModel(_rootFolder, null).Children.ToList();
                }
            }
        }

        public IList Nodes {
            get { return _nodes; }
            private set {
                if (!ReferenceEquals(_nodes, value)) {
                    _nodes = value;
                    NotifyOfPropertyChange(() => Nodes);
                }
            }
        }

        public string Filter {
            get { return _filter; }
            set {
                if (_filter != value) {
                    _filter = value;
                    NotifyOfPropertyChange(() => Filter);
                    UpdateFilter();
                }
            }
        }

        private void UpdateFilter() {
            if (_nodes != null) {
                foreach (var item in _nodes.Cast<ItemViewModel>()) {
                    item.Filter(_filter);
                }
            }
        }

        public bool ShowProjectCount {
            get { return _settings.ShowProjectCount; }
        }

        private DirectoryViewModel CreateDirectoryViewModel(ProjectFolder projectFolder, DirectoryViewModel parent) {
            var viewModel = new DirectoryViewModel(parent, projectFolder);
            if (_settings.IsFlatMode) {
                foreach (var project in new[] { projectFolder }.Flatten(f => f.Projects, f => f.Folders)) {
                    viewModel.Projects.Add(CreateProjectViewModel(project, viewModel));
                }
            } else {
                foreach (var folder in projectFolder.Folders) {
                    viewModel.Directories.Add(CreateDirectoryViewModel(folder, viewModel));
                }
                foreach (var project in projectFolder.Projects) {
                    viewModel.Projects.Add(CreateProjectViewModel(project, viewModel));
                }
            }
            return viewModel;
        }

        private ProjectViewModel CreateProjectViewModel(Project project, DirectoryViewModel parent) {
            return new ProjectViewModel(parent, project);
        }

        public void StartDrag(IDragInfo dragInfo) {
            dragInfo.Data = SelectedItems.OrderBy(m => m.Name);
            dragInfo.Effects = DragDropEffects.Copy;
        }

        public void Dropped(IDropInfo dropInfo) {
        }

        public void DragCancelled() {
        }
    }
}