using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WpfApp1.Detect
{
    public class preprocess
    {
        private static string sub_regx1 = @"([。！？?][”’])";
        private static string sub_regx2 = @"(\.{6})";
        private static string sub_regx3 = @"([。！？?])";
        // 用于中文分句的正则表达式
        private static Regex splitor = new Regex(sub_regx1 + "|" + sub_regx2 + "|" + sub_regx3);
        // 用于去除目录的正则表达式
        private static Regex remove_content = new Regex(@"((目.*?录)[\s\S]*?((结.*?论)|(致.*?谢)|(展望.*?))(\.|…))");

        /*
         中文分句
         */
        public string[] cut_sentence(string text)
        {
            text = remove_content.Replace(text, "");
            string result = splitor.Replace(text, "\n");
            result = result.Replace("\r\n", "\n").Replace(" ", "");
            return result.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        }

    }
}
