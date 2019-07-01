using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using CodeDek.Lib;
using CodeDek.Lib.Mvvm;

namespace SearchAndDeleteEmptyFolders
{
    public sealed class AboutViewModel
    {
        public Brush BackgroundColor => Brushes.LightGoldenrodYellow;

        public byte[] AppIcon => Fun.IconToBytes(Properties.Resources.ic_sad_empty_folders);
        public string Home => "https://github.com/codedek/codedek.searchanddeleteemptyfolders";
        public string Download => "https://github.com/codedek/codedek.searchanddeleteemptyfolders/releases";
        public string Issues => "https://github.com/codedek/codedek.searchanddeleteemptyfolders/issues";
        public string License => "https://github.com/codedek/codedek.searchanddeleteemptyfolders/blob/master/LICENSE";
        public string Changelog => "https://github.com/codedek/codedek.searchanddeleteemptyfolders/blob/master/CHANGELOG.md";
        public string AppName => "SAD Empty Folders";
        public string AppVersion => $"v{Assembly.GetExecutingAssembly().GetName().Version}";
        public string Copyright => "© 2019 CodeDek. All Rights Reserved";
        public string Developer => "Written by CodeDek";

        public Cmd NavigateHomeUrlCmd => new Cmd(() => Process.Start(Home));
        public Cmd NavigateDownloadUrlCmd => new Cmd(() => Process.Start(Download));
        public Cmd NavigateIssuesUrlCmd => new Cmd(() => Process.Start(Issues));
        public Cmd NavigateLicenseUrlCmd => new Cmd(() => Process.Start(License));
        public Cmd NavigateChangelogUrlCmd => new Cmd(() => Process.Start(Changelog));
    }
}
