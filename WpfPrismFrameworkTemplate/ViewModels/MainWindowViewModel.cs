using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfPrismFrameworkTemplate.ViewModels
{
	public class MainWindowViewModel : BindableBase
	{
		private string _title = "CK3创建新伯爵领地工具";
        private string _FileContent = "";
        private string _HighlightedContent = "";
        public DelegateCommand OpenFileCmd { get; private set; }
        public DelegateCommand<string> SearchCmd { get; private set; }
        public MainWindowViewModel()
		{
            OpenFileCmd = new DelegateCommand(OpenFile);
            SearchCmd = new DelegateCommand<string>(SearchContent);
        }
        public string HighlightedContent
        {
            get => _HighlightedContent;
            set => SetProperty(ref _HighlightedContent, value);
        }
        public string FileContent
        {
            get { return _FileContent; }
            set { SetProperty(ref _FileContent, value); }
        }
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        // 打开文件的逻辑
        private void OpenFile()
        {
            // 打开文件对话框
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择文件",
                Filter = "所有文件|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                FileContent = File.ReadAllText(openFileDialog.FileName);
                HighlightedContent = FileContent; // 默认显示为原始内容
            }
        }

        private void SearchContent(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrEmpty(FileContent))
            {
                HighlightedContent = FileContent;
                return;
            }

            // 使用正则表达式高亮匹配的内容
            var regex = new Regex(Regex.Escape(searchTerm), RegexOptions.IgnoreCase);
            HighlightedContent = regex.Replace(FileContent, match => $"<Highlight>{match.Value}</Highlight>");
        }

        private FlowDocument CreateHighlightDocument(string content, string searchTerm)
        {
            var doc = new FlowDocument();
            var paragraph = new Paragraph();

            if (string.IsNullOrEmpty(searchTerm))
            {
                paragraph.Inlines.Add(new Run(content));
            }
            else
            {
                var regex = new Regex(Regex.Escape(searchTerm), RegexOptions.IgnoreCase);
                var matches = regex.Matches(content);

                int lastIndex = 0;
                foreach (Match match in matches)
                {
                    if (match.Index > lastIndex)
                    {
                        paragraph.Inlines.Add(new Run(content.Substring(lastIndex, match.Index - lastIndex)));
                    }

                    var highlightRun = new Run(match.Value)
                    {
                        Background = Brushes.Yellow // 高亮背景
                    };
                    paragraph.Inlines.Add(highlightRun);

                    lastIndex = match.Index + match.Length;
                }

                if (lastIndex < content.Length)
                {
                    paragraph.Inlines.Add(new Run(content.Substring(lastIndex)));
                }
            }

            doc.Blocks.Add(paragraph);
            return doc;
        }

    }
}
