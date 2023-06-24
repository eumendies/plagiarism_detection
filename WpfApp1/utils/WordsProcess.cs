using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WpfApp1.utils
{
    public class WordsProcess
    {
        private uint first_level; // 当前最大的一级标题
        private uint second_level; // 当前最大的一级标题下所属的最大的二级标题
        private uint third_level;

        private static Regex first_match = new Regex("^(#{1})\x20(.*)");  // 用于匹配一级标题的正则表达式
        private static Regex second_match = new Regex("^(#{2})\x20(.*)");  // 用于匹配二级标题的正则表达式
        private static Regex third_match = new Regex("^(#{3})\x20(.*)");  // 用于匹配三级标题的正则表达式

        public WordsProcess()
        {
            first_level = 0;
            second_level = 0;
            third_level = 0;
        }

        public void reset()
        {
            first_level = 0;
            second_level = 0;
            third_level = 0;
        }

        public string readDocx(string path)
        {
            string result = "";
            XWPFDocument docx;
            FileStream stream;
            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.Read);
                docx = new XWPFDocument(stream);
            }
            catch (Exception ex)
            {
                return "";
            }
            IList<XWPFParagraph> paragraphs = docx.Paragraphs;
            foreach (var item in paragraphs)
                result += item.ParagraphText + "\n";
            docx.Close();
            stream.Close();
            return result;
        }

        public void saveDocx(string path, string content)
        {
            string[] paras = content.Split("\n");
            FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write);
            XWPFDocument doc = new XWPFDocument();
            foreach (string para in paras)
            {
                if (first_match.IsMatch(para))
                    addFirstLevel(doc, para.Replace("# ", ""));
                else if (second_match.IsMatch(para))
                    addSecondLevel(doc, para.Replace("## ", ""));
                else if (third_match.IsMatch(para))
                    addThirdLevel(doc, para.Replace("### ", ""));
                else
                    addPara(doc, para);
            }
            doc.Write(stream);
            doc.Close();
            stream.Close();
        }

        public void addFirstLevel(XWPFDocument doc, string text)
        {
            if (text == null || text.Length == 0) return;
            first_level++;
            second_level = 0;
            XWPFParagraph p = doc.CreateParagraph();
            p.Alignment = ParagraphAlignment.CENTER;
            p.SpacingAfterLines = 25;
            XWPFRun r = p.CreateRun();
            r.SetText(first_level.ToString() + "  " + text);
            r.FontFamily = "黑体";
            r.FontSize = 15;
        }

        public void addSecondLevel(XWPFDocument doc, string text)
        {
            if (text == null || text.Length == 0) return;
            second_level++;
            third_level = 0;
            XWPFParagraph p = doc.CreateParagraph();
            p.Alignment = ParagraphAlignment.LEFT;
            p.SpacingAfterLines = 25;
            XWPFRun r = p.CreateRun();
            r.SetText(first_level.ToString() + "." + second_level.ToString() + "  " + text);
            r.FontFamily = "黑体";
            r.FontSize = 12;
        }

        public void addThirdLevel(XWPFDocument doc, string text)
        {
            if (text == null || text.Length == 0) return;
            third_level++;
            XWPFParagraph p = doc.CreateParagraph();
            p.Alignment = ParagraphAlignment.LEFT;
            p.SpacingAfterLines = 25;
            XWPFRun r = p.CreateRun();
            r.SetText(first_level.ToString() + "." + second_level.ToString() + "." + third_level.ToString() + "  " + text);
            r.FontFamily = "黑体";
            r.FontSize = 10.5;
        }

        public void addPara(XWPFDocument doc, string text)
        {
            if (text == null || text.Length == 0) return;
            XWPFParagraph p = doc.CreateParagraph();
            p.Alignment = ParagraphAlignment.LEFT;
            p.IndentationFirstLine = (int)400;
            XWPFRun r = p.CreateRun();
            r.SetText(text);
            r.FontFamily = "宋体";
            r.FontSize = 10.5;
            doc.CreateParagraph(); // 换行
        }
    }
}