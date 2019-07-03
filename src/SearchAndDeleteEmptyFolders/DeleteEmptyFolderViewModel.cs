using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using CodeDek.Lib;
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
        private double _progressValue = 0;
        private bool _isBusy;
        private int _selectionChanged = -1;
        private bool _isParallelSearch;
        private bool _isCanceledSearch;

        public DeleteEmptyFolderViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;

            ClearlCmd.Execute();

            EmptyFolders.CollectionChanged += (s, e) => OnPropertyChanged(nameof(ClearlCmd));
        }

        public ObservableCollection<string> EmptyFolders { get; private set; } = new ObservableCollection<string>();

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
                .Alert(nameof(CancelCmd))
                .Alert(nameof(ClearlCmd));
        }

        public int SelectedIndex
        {
            get => _selectionChanged;
            set => Set(ref _selectionChanged, value)
                .Alert(nameof(DeleteCmd))
                .Alert(nameof(OpenMenuCmd))
                .Alert(nameof(CopyMenuCmd));
        }

        public bool IsParallelSearch
        {
            get => _isParallelSearch;
            set => Set(ref _isParallelSearch, value);
        }

        public bool IsCanceledSearch
        {
            get => _isCanceledSearch;
            set => Set(ref _isCanceledSearch, value)
                .Alert(nameof(CancelCmd));
        }

        public Cmd OpenMenuCmd => new Cmd(() => Process.Start(EmptyFolders[SelectedIndex]), () => SelectedIndex > -1);

        public Cmd CopyMenuCmd => new Cmd(() =>
        {
            Clipboard.SetText(EmptyFolders[SelectedIndex]);
            MessageBox.Show($"{EmptyFolders[SelectedIndex]}", "Path copied to clipboard.");
        }, () => SelectedIndex > -1);

        public Cmd CancelCmd => new Cmd(() =>
        {
            _cts.Cancel();
            IsCanceledSearch = true;
        }, () => IsBusy && !IsCanceledSearch);

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
            IsBusy = false;
            IsCanceledSearch = false;
            ProgressMode = false;
            ProgressValue = 0;
            SelectedIndex = -1;
            _cts = new CancellationTokenSource();
            _token = _cts.Token;
            _token.Register(() => _cts.Dispose());
        }, () => (EmptyFolders.Count > 0 || Source.Length > 0 || Errors > 0 || IsSelectAllChecked) && !IsBusy);

        public Cmd SearchCmd => new Cmd(async () =>
        {
            ClearlCmd.Execute();
            IsBusy = true;
            var sw = new Stopwatch();
            ProgressMode = true;
            sw.Start();

            await Storage.FindEmptyDirectoriesAsync(Source, new Progress<(bool isSuccess, string result)>(h =>
            {
                if (h.isSuccess)
                {
                    EmptyFolders.Add(h.result);
                    Found++;
                }
                else
                {
                    Errors++;
                }
            }), _token, IsParallelSearch);

            sw.Stop();
            ProgressMode = false;

            if (IsParallelSearch && EmptyFolders.Count > 1)
                EmptyFolders = Fun.SortCollection(EmptyFolders, true);

            _mainViewModel.Status = $"{(IsCanceledSearch ? "Partial (since it was canceled by user) " : "")}Search took {sw.ElapsedMilliseconds} milliseconds. {Found} empty folders found.";
            _mainViewModel.Passage = "";
            _mainViewModel.PassageUrl = "";
            IsBusy = false;
        }, () => Source.Length > 0 && !IsBusy);

        public Cmd<ListBox> DeleteCmd => new Cmd<ListBox>(lb =>
        {
            var removed = new List<string>();
            foreach (string item in lb.SelectedItems)
            {
                try
                {
                    Directory.Delete(item);
                    removed.Add(item);
                    Deleted++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Errors++;
                }
            }

            foreach (var item in removed)
            {
                EmptyFolders.Remove(item);
            }

            SelectedIndex = -1;
            _mainViewModel.Status = $"{Deleted} empty folders deleted.";
        }, lb => SelectedIndex > -1 && lb != null);
    }
}
