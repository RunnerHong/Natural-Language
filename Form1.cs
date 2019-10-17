using Baidu.Aip.Nlp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            comboBox3.SelectedIndex = 0;
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
                MessageBox.Show("error:" + e, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                MessageBox.Show("error:" + e, "wrong", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
