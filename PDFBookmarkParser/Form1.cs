using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;

namespace PDFBookmarkParser
{
    public partial class Form1 : Form
    {
        private static IList<Dictionary<string, object>> bookmarks;
        TreeView treeView = new TreeView();
        private static PdfReader pdfReader;

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form1_DragEnter);
            this.DragDrop += new DragEventHandler(Form1_DragDrop);
        }

        private void Init()
        {
            try
            {
                treeView.Nodes.Clear();
                pdfReader.Close();
            }
            catch
            {
            }

        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.

            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                OpenPDF(file);
            }
        }

        private void OpenPDF(string file)
        {
            try
            {
                label2.Text = file;
                pdfReader = new PdfReader(file);
                bookmarks = SimpleBookmark.GetBookmark(pdfReader);
            }
            catch (IOException)
            {
            }
        }

        private void readBookmarksButton_Click(object sender, EventArgs e)
        {
            Init();
            ParseBookmarks();
        }

        private void ParseBookmarks()
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            bw.RunWorkerAsync();
        }

        public TreeNode recursive_search(IList<Dictionary<string, object>> ilist, TreeNode node)
        {
            int bkIndex = 0;

            foreach (Dictionary<string, object> bk in ilist)
            {
                foreach (KeyValuePair<string, object> kvr in bk)
                {
                    if (kvr.Key == "Kids" || kvr.Key == "kids")
                    {
                        IList<Dictionary<string, object>> child = (IList<Dictionary<string, object>>)kvr.Value;
                        recursive_search(child, node.Nodes[bkIndex]);
                    }
                    else if (kvr.Key == "Title" || kvr.Key == "title")
                    {
                        node.Nodes.Add(new TreeNode(kvr.Value.ToString()));
                    }
                    else if (kvr.Key == "Page" || kvr.Key == "page")
                    {
                        //saves page number
                        TreeNode newNode = new TreeNode(kvr.Value.ToString());
                        newNode.ToolTipText = Regex.Match(kvr.Value.ToString(), "[0-9]+").Value;
                        node.Nodes.Add(newNode);
                    }
                }

                bkIndex++;
            }

            return node;
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            TreeNode root = new TreeNode();

            foreach (Dictionary<string, object> bk in bookmarks)    // top-level bookmarks
            {
                foreach (KeyValuePair<string, object> kvr in bk)
                {
                    if (kvr.Key == "Kids" || kvr.Key == "kids")
                    {
                        IList<Dictionary<string, object>> child = (IList<Dictionary<string, object>>)kvr.Value;
                        treeView.Nodes.Add(recursive_search(child, root));
                    }
                    else if (kvr.Key == "Title" || kvr.Key == "title")
                    {
                        root = new TreeNode(kvr.Value.ToString());
                    }
                    else if (kvr.Key == "Page" || kvr.Key == "page")
                    {
                        //saves page number
                        TreeNode newNode = new TreeNode(kvr.Value.ToString());
                        newNode.ToolTipText = Regex.Match(kvr.Value.ToString(), "[0-9]+").Value;
                        treeView.Nodes.Add(newNode);
                    }
                }
            }
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                Init();
                OpenPDF(file);
                ParseBookmarks();
            }
        }

    }
}

