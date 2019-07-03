using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using CodeDek.Lib.Mvvm;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace SearchAndDeleteEmptyFolders
{
    public sealed class DeleteEmptyFolderViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private string _source = "";
        private int _found;
        private int _deleted;
        private int _errors;
        private bool _isSelectAllChecked;

        public DeleteEmptyFolderViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            EmptyFolders.Add(@"c:\win");
            EmptyFolders.Add(@"c:\prd");
            EmptyFolders.Add(@"c:\pass");
            EmptyFolders.Add(@"c:\tmp");

            EmptyFolders.CollectionChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ClearlCmd));
            };
        }

        public ObservableCollection<string> EmptyFolders { get; } = new ObservableCollection<string>();

        public string Source
        {
            get => _source;
            set => Set(ref _source, value)
                .Alert(nameof(ClearlCmd));
        }

        public int Found
        {
            get => _found;
            set => Set(ref _found, value);
        }

        public int Deleted
        {
            get => _deleted;
            set => Set(ref _deleted, value);
        }

        public int Errors
        {
            get => _errors;
            set => Set(ref _errors, value)
                .Alert(nameof(ClearlCmd));
        }

        public bool IsSelectAllChecked
        {
            get => _isSelectAllChecked;
            set => Set(ref _isSelectAllChecked, value)
                .Alert(nameof(ClearlCmd));
        }

        public Cmd SelectSourceCmd => new Cmd(() =>
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select source folder.";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dlg.EnsureValidNames = true;
            dlg.EnsurePathExists = true;
            dlg.ShowPlacesList = true;
            dlg.IsFolderPicker = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Source = dlg.FileName;
            }
        });

        public Cmd<ListBox> SelectAllCmd => new Cmd<ListBox>(lb =>
        {
            if (IsSelectAllChecked)
                lb.SelectAll();
            else
            {
                lb.UnselectAll();
                IsSelectAllChecked = false;
            }
        });

        public Cmd ClearlCmd => new Cmd(() =>
        {
            EmptyFolders.Clear();
            Source = "";
            Found = 0;
            Deleted = 0;
            Errors = 0;
            IsSelectAllChecked = false;
        }, () => EmptyFolders.Count > 0 || Source.Length > 0 || Errors > 0 || IsSelectAllChecked);
    }
}
