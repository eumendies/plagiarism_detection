using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Events;
using WpfApp1.Event;
using WpfApp1.Models;
using Microsoft.Win32;
using System.IO;
using WpfApp1.utils;

namespace WpfApp1.ViewModels
{
    public class WritingViewModel:BindableBase
    {
        private readonly IEventAggregator aggregator;

        private string content;

        private string wordCount;

        private WordsProcess wp;    // 用于读写word

        public DelegateCommand SaveCommand { get; private set; }

        public DelegateCommand LoadCommand { get; private set; }

        public DelegateCommand<String> CountCommand { get; private set; }

        public string Content
        {
            get { return content; }
            set { content = value; RaisePropertyChanged(); }
        }

        public string WordCount
        {
            get { return wordCount; }
            set { wordCount = value; RaisePropertyChanged(); }
        }

        /*
         * 保存写作内容到本地
         */
        private void save()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*.docx)|*.docx|(*.txt)|*.txt";
            if (sfd.ShowDialog() == true)
            {
                string filePath = sfd.FileName;
                string ext = Path.GetExtension(filePath);
                if (ext == ".docx")
                {
                    wp.reset();
                    wp.saveDocx(filePath, content);
                }
                else
                {
                    using (StreamWriter writer = new StreamWriter(filePath))
                    {
                        writer.Write(content);
                    }
                }
            }
        }

        /*
         * 加载文件到写作界面
         */
        private void load()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "(*.txt)|*.txt|(*.docx)|*.docx";
            string lines = null;
            if (ofd.ShowDialog() == true)
            {
                string filename = ofd.FileName;
                string ext = Path.GetExtension(filename);
                if (ext == ".docx")
                    lines = wp.readDocx(filename);
                else
                {
                    using (StreamReader reader = new StreamReader(filename))
                    {
                        lines = reader.ReadToEnd();
                    }
                }
                Content = lines;
                WordCount = lines.Replace(" ", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Length + "字";
            }
        }

        private void count(String text)
        {
            WordCount = text.Replace(" ", "").Replace("\r", "").Replace("\t", "").Replace("\n", "").Length + "字";
        }

        public WritingViewModel(IEventAggregator aggregator)
        {
            this.aggregator = aggregator;
            this.SaveCommand = new DelegateCommand(save);
            this.LoadCommand = new DelegateCommand(load);
            this.CountCommand = new DelegateCommand<string>(count);
            this.wp = new WordsProcess();
            // 订阅RequestEvent，收到消息时发布TransferEvent传输TextBox中的内容
            this.aggregator.GetEvent<RequestEvent>().Subscribe(arg =>
            {
                this.aggregator.GetEvent<TransferEvent>().Publish(content);
            });
        }
    }
}
