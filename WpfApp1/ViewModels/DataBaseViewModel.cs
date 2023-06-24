using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Mvvm;
using WpfApp1.Models;
using MySql.Data;
using MySql.Data.MySqlClient;
using Prism.Commands;
using Microsoft.Win32;
using System.IO;

namespace WpfApp1.ViewModels
{
    public class DataBaseViewModel : BindableBase
    {
        public ObservableCollection<PaperCategory> allPapers;

        public DelegateCommand<String> LoadCommand { get; private set; }

        public DelegateCommand<String> AddCommand { get; private set; }

        public DelegateCommand<String> DeleteCommand { get; private set; }

        private MySqlConnection DBconn;

        private string display;

        public string Display
        {
            get { return display; }
            set { display = value; RaisePropertyChanged(); }
        }

        public ObservableCollection<PaperCategory> AllPapers
        {
            get { return allPapers; }
        }

        /*
         * 连接数据库并加载论文
         */
        private void connectDB()
        {
            string connStr = "server = localhost; user = root; database = paper; port = 3306; password = 159463728; Charset=utf8";
            DBconn = new MySqlConnection(connStr);
            try {
                DBconn.Open(); 
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.ToString()); 
            }
            PaperCategory userPapers = new PaperCategory("我的论文");
            PaperCategory dbPapers = new PaperCategory("数据库中的论文");
            // 读取论文
            string sql = "SELECT * FROM all_papers;";
            MySqlCommand cmd = new MySqlCommand(sql, DBconn);
            MySqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read()) {
                if ((string)reader[3] == "writing")
                    userPapers.Add(new Paper() { ID = reader[0].ToString(), Name = (string)reader[1], Director = "我的论文" });
                else
                    dbPapers.Add(new Paper() { ID = reader[0].ToString(), Name = (string)reader[1], Director = "数据库中的论文" });
            }
            reader.Close();
            allPapers.Add(userPapers);
            allPapers.Add(dbPapers);
        }


        /*
         * 从数据库中加载论文
         */
        private void load(String ID)
        {
            string sql = "SELECT * FROM all_papers where id=" + ID + ";";
            MySqlCommand cmd = new MySqlCommand(sql, DBconn);
            MySqlDataReader reader = cmd.ExecuteReader();
            if (!reader.Read())
                Display = "";
            else 
                Display = (string)reader[2];
            reader.Close();
        }

        /*
         * 添加论文
         */
        private void add(String director)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "(*.txt)|*.txt";
            string lines = null;
            if (ofd.ShowDialog() == true)
            {
                string path = ofd.FileName;
                using (StreamReader reader = new StreamReader(path))
                {
                    lines = reader.ReadToEnd();
                }
                string filename = Path.GetFileName(path);
                // 插入数据库中
                string source = (director == "我的论文") ? "writing" : "upload";
                string sql = String.Format("insert into all_papers (name, content, source) values (\"{0}\", \"{1}\", \"{2}\")", filename, lines, source);
                MySqlTransaction trans = DBconn.BeginTransaction();
                MySqlCommand cmd = new MySqlCommand(sql, DBconn);
                cmd.ExecuteNonQuery();
                trans.Commit();
                // 由于原数据库中最后一行的id+1不一定等于下一个可用id，因此要再进行一次查询以获得新插入的论文的id
                string sql1 = "select * from all_papers order by id desc limit 1;";
                MySqlCommand cmd1 = new MySqlCommand(sql1, DBconn);
                MySqlDataReader r = cmd1.ExecuteReader();
                r.Read();
                // 同时在viewmodel的属性中更新
                if (director == "我的论文")
                    AllPapers[0].Add(new Paper() { ID = r[0].ToString(), Name = filename, Director = "我的论文" });
                else
                    AllPapers[1].Add(new Paper() { ID = r[0].ToString(), Name = filename, Director = "数据库中的论文" });
                r.Close();
                RaisePropertyChanged();
            }
        }

        /*
         * 删除数据库中的论文
         */
        private void delete(String ID)
        {
            // 在数据库中删除
            string sql = String.Format("delete from all_papers where id={0}", ID);
            MySqlCommand cmd = new MySqlCommand(sql, DBconn);
            cmd.ExecuteNonQuery();
            // 在viewmodel中删除
            foreach (Paper paper in AllPapers[0].Papers)
            {
                if (paper.ID == ID)
                {
                    AllPapers[0].Papers.Remove(paper);
                    RaisePropertyChanged();
                    return;
                }
            }
            foreach (Paper paper in AllPapers[1].Papers)
            {
                if (paper.ID == ID)
                {
                    AllPapers[1].Papers.Remove(paper);
                    RaisePropertyChanged();
                    return;
                }
            }
        }

        public DataBaseViewModel()
        {
            Display = "";
            allPapers = new ObservableCollection<PaperCategory>();
            LoadCommand = new DelegateCommand<String>(load);
            AddCommand = new DelegateCommand<String>(add);
            DeleteCommand = new DelegateCommand<String>(delete);
            connectDB();
        }

    }

    public sealed class PaperCategory
    {
        public PaperCategory(string name, params Paper[] papers)
        {
            Name = name;
            Papers = new ObservableCollection<Paper>(papers);
        }

        public string Name { get; }

        public ObservableCollection<Paper> Papers { get; }

        public void Add(Paper p)
        {
            Papers.Add(p);
        }
    }
}
