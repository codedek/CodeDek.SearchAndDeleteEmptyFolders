﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CodeDek.Lib.FortuneCookie;
using CodeDek.Lib.Mvvm;

namespace SearchAndDeleteEmptyFolders
{
    public sealed class MainViewModel : ObservableObject
    {
        private Brush _statusColor = Brushes.Black;
        private Brush _backgroundColor = Brushes.LightGoldenrodYellow;
        private string _passageUrl;
        private string _passage;
        private string _status;

        public MainViewModel()
        {
            var (text, passage, url) = CookieGenerator.GenerateCookie(CookieType.Promises);
            Status = text;
            Passage = passage;
            PassageUrl = url;
            DeleteEmptyFolderViewModel = new DeleteEmptyFolderViewModel(this);
        }

        public string Title { get; } = "CodeDek's SAD Empty Folders";
        public int MinHeight { get; } = 480;
        public int MinWidth { get; } = 720;
        public AboutViewModel AboutViewModel { get; } = new AboutViewModel();
        public DeleteEmptyFolderViewModel DeleteEmptyFolderViewModel { get; }

        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public string Passage
        {
            get => _passage;
            set => Set(ref _passage, value);
        }

        public string PassageUrl
        {
            get => _passageUrl;
            set => Set(ref _passageUrl, value);
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set => Set(ref _statusColor, value);
        }

        public Brush BackgroundColor
        {
            get => _backgroundColor;
            set => Set(ref _backgroundColor, value);
        }

        public Cmd GoToUrlCmd => new Cmd(() => Process.Start(PassageUrl));
    }
}
