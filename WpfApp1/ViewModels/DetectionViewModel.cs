using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using Prism.Commands;
using System.IO;
using Microsoft.Win32;
using Prism.Events;
using WpfApp1.Event;
using WpfApp1.Detect;
using MySql.Data;
using MySql.Data.MySqlClient;
using WpfApp1.utils;

namespace WpfApp1.ViewModels
{
    public class DetectionViewModel : BindableBase
    {
        private Detector detector;
        private Paper src;
        private Paper sus;
        public ObservableCollection<PaperCategory> allPapers; // 数据库中的论文
        private WordsProcess wp;
        private string result_src;
        private string result_sus;
        private bool resultDialogOpen;
        private bool processDialogOpen;
        private bool rightDrawerOpen;
        public DelegateCommand<String> OpenFileCommand { get; private set; }
        public DelegateCommand LoadWritingCommand { get; private set; }
        public DelegateCommand DetectCommand { get; private set; }
        public DelegateCommand OpenResultDialogCommand { get; private set; }
        public DelegateCommand CloseResultDialogCommand { get; private set; }
        public DelegateCommand AccessDBCommand { get; private set; }
        public DelegateCommand<String> LoadDBCommand { get; private set; }

        private readonly IEventAggregator aggregator;

        /*
         * Src TextBox绑定的对象
         */
        public Paper Src
        {
            get { return src; }
            set { src = value; RaisePropertyChanged(); }
        }

        /*
         * Sus TextBox绑定的对象
         */
        public Paper Sus
        {
            get { return sus; }
            set { sus = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<PaperCategory> AllPapers
        {
            get { return allPapers; }
        }

        public string ResultSrc
        {
            get { return result_src; }
            set { result_src = value; RaisePropertyChanged(); }
        }

        public string ResultSus
        {
            get { return result_sus; }
            set { result_sus = value; RaisePropertyChanged(); }
        }

        public bool ResultDialogOpen
        {
            get { return resultDialogOpen; }
            set { resultDialogOpen = value; RaisePropertyChanged(); }
        }

        private void openResultDialog()
        {
            ResultDialogOpen = true;
        }

        private void closeResultDialog()
        {
            ResultDialogOpen = false;
        }

        public bool ProcessDialogOpen
        {
            get { return processDialogOpen; }
            set { processDialogOpen = value; RaisePropertyChanged(); }
        }

        private void openProcessDialog()
        {
            ProcessDialogOpen = true;
        }

        private void closeProcessDialog()
        {
            ProcessDialogOpen = false;
        }

        public bool RightDrawerOpen
        {
            get { return rightDrawerOpen; }
            set { rightDrawerOpen = value; RaisePropertyChanged(); }
        }

        /*
         * 打开文件对话框，加载文件   
         */
        private void getFile(String id)
        {
            string content = null;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "(*.docx)|*.docx|(*.txt)|*.txt";
            if (ofd.ShowDialog() == true)
            {
                string fileName = ofd.FileName;
                string ext = Path.GetExtension(fileName);
                if (ext == ".docx")
                {
                    content = wp.readDocx(fileName);
                }
                else
                {
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }
            if (content == null) return;
            // 根据传入的id决定要将内容显示在哪个TextBox中
            if (id == "0")
                Src = new Paper() { Content = content};
            else 
                Sus = new Paper() { Content = content};
        }

        private void accessDB()
        {
            string connStr = "server = localhost; user = root; database = paper; port = 3306; password = 159463728; Charset=utf8";
            MySqlConnection DBconn = new MySqlConnection(connStr);
            DBconn.Open();
            allPapers.Clear();
            allPapers.Add(new PaperCategory("我的论文"));
            allPapers.Add(new PaperCategory("数据库中的论文"));
            // 读取论文
            string sql = "SELECT * FROM all_papers;";
            MySqlCommand cmd = new MySqlCommand(sql, DBconn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if ((string)reader[3] == "writing")
                    allPapers[0].Add(new Paper() { ID = reader[0].ToString(), Name = (string)reader[1], Director = "我的论文" });
                else
                    allPapers[1].Add(new Paper() { ID = reader[0].ToString(), Name = (string)reader[1], Director = "数据库中的论文" });
            }
            reader.Close();
            DBconn.Close();
            RightDrawerOpen = true;
            RaisePropertyChanged();
        }

        /*
         * 从写作界面加载内容至查重界面，发表请求事件
        */
        private void loadFromWriting()
        {
            aggregator.GetEvent<RequestEvent>().Publish("");
        }

        /*
         * 从数据库中加载内容至查重界面
         */
        private void loadFromDB(String ID)
        {
            string connStr = "server = localhost; user = root; database = paper; port = 3306; password = 159463728; Charset=utf8";
            MySqlConnection DBconn = new MySqlConnection(connStr);
            DBconn.Open();
            string sql = String.Format("SELECT content FROM all_papers where ID={0};", ID);
            MySqlCommand cmd = new MySqlCommand(sql, DBconn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                Sus = new Paper() { Content = (string)reader[0] };
            }
            reader.Close();
            DBconn.Close();
        }

        /*
         * 展示处理动画并进行检测
         */
        private void show_detect()
        {
            if (Src.Content == null || Sus.Content == null) return;
            openProcessDialog();
            Task.Run(() => detect());
        }

        /*
         * 进行查重
         */
        private void detect()
        {
            var (similar_sen_sus, similar_sen_src, similarity_list) = detector.detect(Sus.Content, Src.Content);
            string src_result = "";
            string sus_result = "";
            for (int i = 0; i < similarity_list.Count; i++)
            {
                string sim = "\n(相似度：" + similarity_list[i].ToString("f3") + ")\n\n";
                src_result += similar_sen_src[i] + sim;
                sus_result += similar_sen_sus[i] + sim;
            }
            ResultSrc = src_result;
            ResultSus = sus_result;
            closeProcessDialog();
            openResultDialog();
        }

        public DetectionViewModel(IEventAggregator aggregator)
        {
            detector = new Detector();
            src = new Paper();
            sus = new Paper();
            allPapers = new ObservableCollection<PaperCategory>();
            result_src = "";
            result_sus = "";
            resultDialogOpen = false;
            processDialogOpen = false;
            rightDrawerOpen = false;
            wp = new WordsProcess();
            OpenFileCommand = new DelegateCommand<String>(getFile);
            LoadWritingCommand = new DelegateCommand(loadFromWriting);
            DetectCommand = new DelegateCommand(show_detect);
            OpenResultDialogCommand = new DelegateCommand(openResultDialog);
            CloseResultDialogCommand = new DelegateCommand(closeResultDialog);
            AccessDBCommand = new DelegateCommand(accessDB);
            LoadDBCommand = new DelegateCommand<String>(loadFromDB);
            this.aggregator = aggregator;
            // 订阅传输事件，将写作界面传来的数据设为Src Textbox的内容
            this.aggregator.GetEvent<TransferEvent>().Subscribe(arg =>
            {
                Src = new Paper { Content = arg };
            });
        }
    }
}
