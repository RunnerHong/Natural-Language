using Baidu.Aip.Nlp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class naturalLanguage : Form
    {
        // 设置APPID/AK/SK
        // private static string APP_ID = "17549812";
        private static string API_KEY = "UY9ge9dXGdfgcdvnfZElBBZ9";
        private static string SECRET_KEY  = "eEF7C5fPp6xvatvCMWHxWczfT2ZGKjI3";
        Nlp client = new Nlp(API_KEY, SECRET_KEY);

        JObject posTranslation = JObject.Parse(
            "{'n':'普通名词','f':'方位名词','s':'处所名词','t':'时间名词','nr':'人名','ns':'地名'," +
            "'nt':'机构团体名','nw':'作品名','nz':'其他专名','v':'普通动词','vd':'动副词','vn':'名动词','a':'形容词'" +
            ",'ad':'副形词','an':'名形词','d':'副词','m':'数量词','q':'量词','r':'代词','p':'介词'" +
            ",'c':'连词','u':'助词','xc':'其他虚词','w':'标点符号','PER':'人名','LOC':'地名','ORG':'机构名'" +
            ",'TIME':'时间'}");

        private readonly Dictionary<string, string> DeprelTable = new Dictionary<string, string>
        {
            {
                "ATT",
                "定中关系"
            },
            {
                "QUN",
                "数量关系"
            },
            {
                "COO",
                "并列关系"
            },
            {
                "APP",
                "同位关系"
            },
            {
                "ADJ",
                "附加关系"
            },
            {
                "VOB",
                "动宾关系"
            },
            {
                "POB",
                "介宾关系"
            },
            {
                "SBV",
                "主谓关系"
            },
            {
                "SIM",
                "比拟关系"
            },
            {
                "TMP",
                "时间关系"
            },
            {
                "LOC",
                "处所关系"
            },
            {
                "DE",
                "“的”字结构"
            },
            {
                "DI",
                "“地”字结构"
            },
            {
                "DEI",
                "“得”字结构"
            },
            {
                "SUO",
                "“所”字结构"
            },
            {
                "BA",
                "“把”字结构"
            },
            {
                "BEI",
                "“被”字结构"
            },
            {
                "ADV",
                "状中结构"
            },
            {
                "CMP",
                "动补结构"
            },
            {
                "DBL",
                "兼语结构"
            },
            {
                "CNJ",
                "关联词"
            },
            {
                "CS",
                "关联结构"
            },
            {
                "MT",
                "语态结构"
            },
            {
                "VV",
                "连谓结构"
            },
            {
                "HED",
                "核心"
            },
            {
                "FOB",
                "前置宾语"
            },
            {
                "DOB",
                "双宾语"
            },
            {
                "TOP",
                "主题"
            },
            {
                "IS",
                "独立结构"
            },
            {
                "IC",
                "独立分句"
            },
            {
                "DC",
                "依存分句"
            },
            {
                "VNV",
                "叠词关系"
            },
            {
                "YGC",
                "一个词"
            },
            {
                "WP",
                "标点"
            },
            {
                "",
                ""
            }
        };

        public naturalLanguage()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 9;
            client.Timeout = 60000;  // 修改百度接口超时时间
        }

        public void Lexer()
        {
            var text = textBox1.Text;
            JObject result = new JObject();
            try
            {
                result = client.Lexer(text);
            }
            catch(Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }            
            RenderLexerResult(result);
        }

        public void RenderLexerResult(JObject result)
        {
            result.TryGetValue("items", out JToken items);
            int index = 1;
            foreach (var item in items)
            {
                JToken pos = null;
                posTranslation.TryGetValue(item["pos"].ToString(), out pos);
                if (pos == null)
                {
                    posTranslation.TryGetValue(item["ne"].ToString(), out pos);
                }
                string basic_words = string.Join("/", item["basic_words"]);
                listView1.Items.Add(new ListViewItem(
                    new string[] {index.ToString(), item["item"].ToString(), pos.ToString(), basic_words}));
                index += 1;
            }
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Lexer();
        }

        public void DepParser()
        {
            var text = textBox1.Text;
            JObject result = new JObject();
            var options = new Dictionary<string, object>{
                {"mode", comboBox1.SelectedIndex}
            };
            try
            {
                result = client.DepParser(text, options);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RenderDepResult(result);
        }

        public void RenderDepResult(JObject result)
        {
            result.TryGetValue("items", out JToken items);
            Dictionary<int, List<JToken>> indexingItems = new Dictionary<int, List<JToken>>();
            // generate indexing items dict
            foreach (var item in items)
            {
                int head = item.Value<int>("head");
                if (!indexingItems.ContainsKey(head))
                {
                    List<JToken> list = new List<JToken>();
                    list.Add(item);
                    indexingItems.Add(head, list);
                }
                else
                {
                    indexingItems[head].Add(item);
                }
            }
            // render the result
            void Recursion(int index=0, TreeNode node=null)
            {
                if (!indexingItems.ContainsKey(index))
                {
                    return;
                }
                foreach (var item in indexingItems[index])
                {
                    posTranslation.TryGetValue(item["postag"].ToString(), out JToken pos);
                    TreeNode newNode = new TreeNode(string.Format("{0}({1})", item["word"].ToString(), DeprelTable[item["deprel"].ToString()]))
                    {                    
                        ToolTipText = pos.ToString()
                    };
                    if (index == 0)
                    {
                        treeView1.Nodes.Add(newNode);
                    }
                    else
                    {
                        node.Nodes.Add(newNode);
                    }
                    Recursion(item.Value<int>("id"), newNode);
                }
            }
            treeView1.Nodes.Clear();
            Recursion();
            treeView1.ExpandAll();
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            DepParser();
        }

        public void DnnlmCn()
        {
            var text = textBox1.Text;
            JObject result = new JObject();
            try
            {
                result = client.DnnlmCn(text);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RenderDnnlmCnResult(result);
        }

        public void RenderDnnlmCnResult(JObject result)
        {
            result.TryGetValue("items", out JToken items);
            result.TryGetValue("ppl", out JToken ppl);
            textBox3.Text = ppl.ToString();
            int index = 1;
            foreach (var item in items)
            {
                listView2.Items.Add(new ListViewItem(
                    new string[] {index.ToString(), item["word"].ToString(), item["prob"].ToString() }));
                index += 1;
            }
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            DnnlmCn();
        }

        public void Simnet()
        {
            var text1 = textBox4.Text;  
            var text2 = textBox5.Text;
            JObject result = new JObject();

            var options = new Dictionary<string, object>{
                {"model", comboBox2.SelectedItem}
            };
            try
            {
                result = client.Simnet(text1, text2, options);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            result.TryGetValue("score", out JToken score);
            textBox6.Text = score.ToString();
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            Simnet();
        }

        public void CommentTag()
        {
            var text = textBox7.Text.Trim();
            JObject result = new JObject();

            var options = new Dictionary<string, object>{
                {"type", comboBox3.SelectedIndex == 2 ? comboBox3.SelectedIndex : comboBox3.SelectedIndex + 1}
            };
            try
            {
                result = client.CommentTag(text, options);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RenderCommentTagResult(result);
        }

        public void RenderCommentTagResult(JObject result)
        {
            if (result.TryGetValue("error_msg", out JToken errorMsg))
            {
                richTextBox1.Text = errorMsg.ToString();
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (JToken item in result["items"])
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine("---------------------------------------");
                    }
                    stringBuilder.AppendLine(string.Format("观点倾向：{0}\r\n短句摘要：{1}\r\n匹配属性词：{2}\r\n匹配描述词：{3}", new object[]
                    {
                            (item.Value<int>("sentiment") == 0) ? "消极" : ((result.Value<int>("sentiment") == 1) ? "中性" : "积极"),
                            item["abstract"],
                            item["prop"],
                            item["adj"]
                    }));
                }
                richTextBox1.Text = stringBuilder.ToString();
            }
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            CommentTag();
        }

        public void SentimentClassify()
        {
            var text = textBox7.Text.Trim();
            JObject result = new JObject();

            try
            {
                result = client.SentimentClassify(text);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RenderSentimentClassifyResult(result);
        }

        public void RenderSentimentClassifyResult(JObject result)
        {
            if (result.TryGetValue("error_msg", out JToken errorMsg))
            {
                richTextBox1.Text = errorMsg.ToString();
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (JToken item in result["items"])
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.AppendLine("---------------------------------------");
                    }
                    stringBuilder.AppendLine(string.Format("情感倾向：{0}\r\n置信度：{1}", new object[]
                    {
                            (item.Value<int>("sentiment") == 0) ? "消极" : ((result.Value<int>("sentiment") == 1) ? "中性" : "积极"),
                            item["confidence"]
                    }));
                }
                richTextBox1.Text = stringBuilder.ToString();
            }
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            SentimentClassify();
        }

        public void WordEmbedding()
        {
            var word = textBox9.Text.Trim();
            JObject result = new JObject();

            try
            {
                result = client.WordEmbedding(word);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RenderWordEmbeddingResult(result);
        }

        public void RenderWordEmbeddingResult(JObject result)
        {
            if (result.TryGetValue("error_msg", out JToken errorMsg))
            {
                richTextBox2.Text = errorMsg.ToString();
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (JToken item in result["vec"])
                {
                    stringBuilder.AppendLine(item.ToString());
                }
                richTextBox2.Text = stringBuilder.ToString();
            }
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            WordEmbedding();
        }

        public void WordSimEmbedding()
        {
            var word1 = textBox11.Text.Trim();
            var word2 = textBox12.Text.Trim();
            JObject result = new JObject();
            try
            {
                result = client.WordSimEmbedding(word1, word2);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            result.TryGetValue("score", out JToken score);
            textBox13.Text = score.ToString();
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            WordSimEmbedding();
        }

        public void Keyword()
        {
            var title = textBox14.Text.Trim();

            var content = textBox15.Text.Trim();

            JObject result = new JObject();
            try
            {
                result = client.Keyword(title, content);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (JToken item in result["items"])
            {
                stringBuilder.AppendLine(string.Format("{0}:{1}", item["tag"].ToString(), item["score"].ToString()));
            }
            textBox16.Text = stringBuilder.ToString();
        }

        private void Button9_Click(object sender, EventArgs e)
        {
            Keyword();
        }

        public void Topic()
        {
            var title = textBox14.Text.Trim();

            var content = textBox15.Text.Trim();

            JObject result = new JObject();
            try
            {
                result = client.Topic(title, content);
            }
            catch (Exception e)
            {
                MessageBox.Show("error:" + e.Message, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            RenderTopicResult(result);
        }

        public void RenderTopicResult(JObject result)
        {
            if (result.TryGetValue("error_msg", out JToken errorMsg))
            {
                richTextBox1.Text = errorMsg.ToString();
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("一级分类结果：");
                foreach (JToken lv1 in ((IEnumerable<JToken>)result["item"]["lv1_tag_list"]))
                {
                    stringBuilder.AppendLine(string.Format("    {0}：{1}", lv1["tag"], lv1["score"]));
                }
                stringBuilder.AppendLine("二级分类结果：");
                foreach (JToken lv2 in ((IEnumerable<JToken>)result["item"]["lv2_tag_list"]))
                {
                    stringBuilder.AppendLine(string.Format("    {0}：{1}", lv2["tag"], lv2["score"]));
                }
                textBox16.Text = stringBuilder.ToString();
            }
        }

        private void Button10_Click(object sender, EventArgs e)
        {
            Topic();
        }
    }
}
