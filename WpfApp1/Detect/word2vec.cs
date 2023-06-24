using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WpfApp1.Detect
{
    public class word2vec
    {
        private Dictionary<char, List<double>> dict;

        /*
         传入一个字，获取对应的字向量
         */
        public List<double> getVec(char character)
        {
            try
            {
                return dict[character];
            }
            catch (Exception)
            {
                if (character != '\n' && character != '\r')
                    Console.WriteLine("DEBUG: word2vec exception，the given key " + character + " was not present in the dict");
                return null;
            }
        }

        /*
         读取由神经网络创建的word2vec字典
         */
        public word2vec()
        {
            dict = new Dictionary<char, List<double>>();
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            string json = File.ReadAllText(@"../../../Parameter/word2vec.json", Encoding.GetEncoding("GB18030"));
            dict = JsonConvert.DeserializeObject<Dictionary<char, List<double>>>(json);
        }
    }
}
