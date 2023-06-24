using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NumSharp;
using System.Diagnostics;

namespace WpfApp1.Detect
{
    public class Detector
    {
        private static word2vec dict = new word2vec();
        private static preprocess splitor = new preprocess();
        // 字向量维度
        private static int embedding_size = 200;

        /*
         将一个向量转换为单位向量
         */
        public NDArray normalize(NDArray vec)
        {
            Debug.Assert(vec.dtype == typeof(Double));
            double norm = 0;
            foreach (double val in vec)
                norm += val * val;
            if (norm == 0) return vec;
            return vec / np.sqrt(norm);
        }

        /*
         获取一个句子的向量，形状：[embedding_size]
         */
        public NDArray get_sen_vec(string sen)
        {
            var sen_vec = np.zeros(embedding_size);
            for (int i = 0; i < sen.Length; i++)
            {
                // 句向量由每个字向量对应维度上元素简单加和得到
                List<double> vec = dict.getVec(sen[i]);
                var char_vec = vec != null ? np.array(dict.getVec(sen[i])) : np.zeros(embedding_size);
                sen_vec = np.add(sen_vec, char_vec);
            }
            return normalize(sen_vec);
        }

        /*
         获取一段文本的向量，形状：[sen_list.Length, embedding_size]
         */
        public NDArray get_doc_vec(string[] sen_list)
        {
            NDArray[] sen_vec_list = new NDArray[sen_list.Length];
            for (int i = 0; i < sen_list.Length; i++)
            {
                var sen_vec = get_sen_vec(sen_list[i]).reshape(-1, embedding_size);
                sen_vec_list[i] = sen_vec;
            }
            return np.concatenate(sen_vec_list);
        }

        /*
         输入两篇文档，一篇为可能有抄袭嫌疑的文档，一篇为源文档，找出其中相似的语句
         */
        public (List<string>, List<string>, List<double>) detect(string sus_doc, string src_doc)
        {
            string[] sus_sens = splitor.cut_sentence(sus_doc);
            string[] src_sens = splitor.cut_sentence(src_doc);
            NDArray sus_matrix = get_doc_vec(sus_sens);
            NDArray src_matrix = get_doc_vec(src_sens);
            NDArray similarity_matrix = np.matmul(sus_matrix, src_matrix.T);
            // 余弦相似度大于0.85判断为存在抄袭嫌疑
            NDArray detect_matrix = np.maximum(similarity_matrix - 0.85, 0);
            // 取出相似的句子
            List<string> similar_sen_sus = new List<string>();
            List<string> similar_sen_src = new List<string>();
            List<double> similarity_list = new List<double>();
            for (int i = 0; i < detect_matrix.shape[0]; i++)
            {
                for (int j = 0; j < detect_matrix.shape[1]; j++)
                {
                    if ((double)detect_matrix[i, j] > 0)
                    {
                        similar_sen_sus.Add(sus_sens[i]);
                        similar_sen_src.Add(src_sens[j]);
                        similarity_list.Add(similarity_matrix[i, j]);
                    }
                }
            }
            return (similar_sen_sus, similar_sen_src, similarity_list);
        }
    }
}
