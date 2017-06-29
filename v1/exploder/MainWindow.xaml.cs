using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Exploder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _scan.OnCompleted += _scan_OnCompleted;
            chkFoundFolders.IsChecked = false;
        }
        
        private void FolderOpen_Click(object sender, RoutedEventArgs e)
        {
            string startPath = "";
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    startPath = dialog.SelectedPath;
                }
            }
            txtFolderPath.Text = startPath;
            MessageBox.Show(startPath);
        }
        
        private void Search_Click(object sender, RoutedEventArgs e)
        {
            doSearch(txtFolderPath.Text, txtSearchString.Text);
        }

        

        /*************** Scan ***********/

        private Scan _scan = new Scan();
        public TreeViewItem rootTreeItem = null;

        private void doSearch(string rootFolderName, string searchString = "")
        {
            // clear all nodes
            if (rootTreeItem != null)
                rootTreeItem.Items.Clear();
            tvMain.Items.Clear();            
            // Add root node
            TreeViewItem tvi = new TreeViewItem() { Header = "Scan" };
            tvi.IsExpanded = true;
            rootTreeItem = tvi;
            tvMain.Items.Add(tvi);

            //FolderInfo fi = new FolderInfo() { FolderName = rootFolderName };
            //addToTreeView(fi);


            // start scan
            Scan.SearchString = searchString;
            Scan.FoundFolderOnly = (bool) chkFoundFolders.IsChecked;         
            Thread trd = new Thread(new ParameterizedThreadStart(runScan));
            trd.Start(txtFolderPath.Text);
        }

        private void runScan(object startFolder)
        {
            string folderPath = (string)startFolder;
            _scan.GetFiles(folderPath);

        }

        private void scanCompleted(object sender, EventArgs e)
        {
            string buttonState = (string)sender;
            if (buttonState == "START")
                btnSearch.Background = new SolidColorBrush();
            if (buttonState == "END")
                btnSearch.Background = new SolidColorBrush();
        }

        private void _scan_OnCompleted(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new EventHandler(searchCompleted), new object[] { sender, e });
        }

        
        private void searchCompleted(object sender, EventArgs e)
        {
            FolderInfo fi = null;
            if (sender != null)
            {
                fi = (FolderInfo)sender;
                addToTreeView(fi);
                //MessageBox.Show(string.Format("{0} : {1}", fi.FolderName, Helper.SizeSuffix(fi.Size)));
            }
        }


        private void addToTreeView(FolderInfo fi)
        {
            TreeViewItem treeItem = null;
            treeItem = new TreeViewItem();
            treeItem.Header = string.Format("{0} : {1}", fi.FolderName, Helper.SizeSuffix(fi.Size)) ;
            treeItem.Tag = fi;
            treeItem.IsExpanded = true;

            if (string.IsNullOrEmpty(fi.ParentFolder))
                rootTreeItem.Items.Add(treeItem);
            else
            {
                TreeViewItem parentTvi = findNode(rootTreeItem.Items, fi.ParentFolder);
                if (parentTvi == null)
                    rootTreeItem.Items.Add(treeItem);
                else
                {
                    if (fi.Type == ItemType.File)
                        treeItem.Header = string.Format("- {0} : {1}", fi.FolderName.Replace(fi.ParentFolder, ""), Helper.SizeSuffix(fi.Size));
                    else
                        treeItem.Header = string.Format("{0} : {1}", fi.FolderName.Replace(fi.ParentFolder, ""), Helper.SizeSuffix(fi.Size));
                    FolderInfo pfi = (FolderInfo)parentTvi.Tag;
                    pfi.Size += fi.Size;
                    parentTvi.Items.Add(treeItem);
                    parentTvi.Header = string.Format("{0} : {1}", pfi.FolderName, Helper.SizeSuffix(pfi.Size));
                }
            }
        }

        private TreeViewItem findNode(ItemCollection items, string searchParent)
        {
            TreeViewItem oResult = null;

            foreach (var oItem in items)
            {
                TreeViewItem oTreeViewItem = (TreeViewItem)oItem;

                if (((FolderInfo)oTreeViewItem.Tag).FolderName == searchParent) { oResult = oTreeViewItem; break; }

                if (oTreeViewItem.Items.Count > 0)
                {
                    oResult = findNode(oTreeViewItem.Items, searchParent);

                    if (oResult != null) { break; }
                }
            }
            return oResult;
        }
    }
}
