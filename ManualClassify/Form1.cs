using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using data;
using data.Model;

namespace ManualClassify
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private int totalCount,nowIndex;
        private List<MSinaAccount> accounts;

        private void Form1_Load(object sender, EventArgs e)
        {
            nowIndex = 0;
            accounts = new List<MSinaAccount>();
            using (var db = new sweepEntities1())
            {
                var acs = db.sinaaccounts
                    .Where(ac => ac.isWaste == 0).ToList();
                foreach (var ac in acs)
                {
                    accounts.Add(new MSinaAccount
                    {
                        id = ac.id,
                        uid = ac.uid,
                        isWaste = ac.isWaste,
                        name = ac.name,
                        followers_count = ac.followers_count,
                        friends_count = ac.friends_count,
                        statuses_count = ac.statuses_count,
                        favourites_count = ac.favourites_count,
                        posts = MPost.ConvertFromPost(ac.posts.ToList())
                    });
                }
                totalCount = accounts.Count;
            }
            refreshForm();
        }

        private void refreshForm()
        {
            bool select = true;
            while ((nowIndex < totalCount) && select)
            {
                if (accounts[nowIndex].posts.Count > 0)
                {
                    select = false;
                    userNameLabel.Text = accounts[nowIndex].name;
                    postLayoutPanel.Controls.Clear();
                    foreach (var p in accounts[nowIndex].posts)
                    {
                        var textLabel = new Label();
                        textLabel.AutoSize = true;
                        textLabel.MaximumSize = new System.Drawing.Size(490, 0);
                        textLabel.MinimumSize = new System.Drawing.Size(490, 0);
                        textLabel.Text = p.text;
                        postLayoutPanel.Controls.Add(textLabel);
                        if (p.reason.Length > 0)
                        {
                            var reasonLabel = new Label();
                            reasonLabel.AutoSize = true;
                            reasonLabel.MaximumSize = new System.Drawing.Size(490, 0);
                            reasonLabel.MinimumSize = new System.Drawing.Size(490, 0);
                            reasonLabel.Text = "  ->"+p.reason;
                            postLayoutPanel.Controls.Add(reasonLabel);
                        }
                        var newLineLabel = new Label();
                        newLineLabel.Text = "\r\n";
                        newLineLabel.MaximumSize = new System.Drawing.Size(490, 20);
                        newLineLabel.MinimumSize = new System.Drawing.Size(490, 0);
                        postLayoutPanel.Controls.Add(newLineLabel);
                    }
                    countLabel.Text = (nowIndex + 1) + "/" + (totalCount);
                    urlLabel.Text = @"http://weibo.com/u/" + accounts[nowIndex].uid;
                }
                else
                {
                    nowIndex++;
                }
            }
            if (nowIndex == totalCount)
            {
                isUserButton.Enabled = false;
                isWasteButton.Enabled = false;
                passButton.Enabled = false;
                countLabel.Text = (nowIndex) + "/" + (totalCount);
            }
        }

        private void urlLabel_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(((Label)sender).Text);
        }

        private void isUserButton_Click(object sender, EventArgs e)
        {
            using (var db = new sweepEntities1())
            {
                var id = accounts[nowIndex].id;
                var ac = db.sinaaccounts.Where(a => a.id == id).SingleOrDefault();
                if (ac != null)
                {
                    ac.isWaste = 1;
                }
                db.SaveChanges();
            }
            nowIndex++;
            refreshForm();
        }

        private void isWasteButton_Click(object sender, EventArgs e)
        {
            using (var db = new sweepEntities1())
            {
                var id = accounts[nowIndex].id;
                var ac = db.sinaaccounts.Where(a => a.id == id).SingleOrDefault();
                if (ac != null)
                {
                    ac.isWaste = 2;
                }
                db.SaveChanges();
            }
            nowIndex++;
            refreshForm();
        }

        private void passButton_Click(object sender, EventArgs e)
        {
            nowIndex++;
            refreshForm();
        }



    }
}
