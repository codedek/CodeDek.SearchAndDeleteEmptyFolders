using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using CodeDek.Lib.Mvvm;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace SearchAndDeleteEmptyFolders
{
    public sealed class DeleteEmptyFolderViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;
        private CancellationTokenSource _cts = new CancellationTokenSource();
        private CancellationToken _token;
        private string _source = "";
        private int _found;
        private int _deleted;
        private int _errors;
        private bool _isSelectAllChecked;
        private bool _progressMode;
        private double _progressValue = 45;
        private bool _isBusy;
        private int _selectionChanged = -1;

        public DeleteEmptyFolderViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
            EmptyFolders.Add(@"c:\win");
            EmptyFolders.Add(@"c:\prd");
            EmptyFolders.Add(@"c:\pass");
            EmptyFolders.Add(@"c:\tmp");

            _token = _cts.Token;
            _token.Register(() => _cts.Dispose());

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
                .Alert(nameof(ClearlCmd))
                .Alert(nameof(SearchCmd));
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

        public bool ProgressMode
        {
            get => _progressMode;
            set => Set(ref _progressMode, value);
        }

        public double ProgressValue
        {
            get => _progressValue;
            set => Set(ref _progressValue, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => Set(ref _isBusy, value)
                .Alert(nameof(SearchCmd))
                .Alert(nameof(CancelCmd));
        }

        public int SelectedIndex
        {
            get => _selectionChanged;
            set => Set(ref _selectionChanged, value)
                .Alert(nameof(DeleteCmd));
        }

        public Cmd SelectSourceCmd => new Cmd(() =>
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = "Select source folder.";
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
            Found = 0;
            Deleted = 0;
            Errors = 0;
            IsSelectAllChecked = false;
        }, () => EmptyFolders.Count > 0 || Source.Length > 0 || Errors > 0 || IsSelectAllChecked);

        public Cmd SearchCmd => new Cmd(() =>
        {
            ClearlCmd.Execute();
            IsBusy = true;
            var sw = new Stopwatch();

            // while search
            // update progress
            // update found
            // Add to list
            // update errors

            _mainViewModel.Status = $"Search took {sw.ElapsedMilliseconds} milliseconds. {Found} empty folders found.";
            _mainViewModel.Passage = "";
            _mainViewModel.PassageUrl = "";
            IsBusy = false;
        }, () => Source.Length > 0 && !IsBusy);

        public Cmd CancelCmd => new Cmd(() => _cts.Cancel(), () => IsBusy);

        public Cmd<ListBox> DeleteCmd => new Cmd<ListBox>(lb =>
        {
            foreach (string item in lb.SelectedItems)
            {
                try
                {
                    Directory.Delete(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Errors++;
                }
            }

            SelectedIndex = -1;
            _mainViewModel.Status = $"{Deleted} empty folders deleted.";
        }, lb => SelectedIndex > -1 && lb != null);
    }
}
