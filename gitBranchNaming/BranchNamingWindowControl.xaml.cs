//------------------------------------------------------------------------------
// <copyright file="BranchNamingWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace gitBranchNaming
{
    using EnvDTE80;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    /// <summary>
    /// Interaction logic for BranchNamingWindowControl.
    /// </summary>
    public partial class BranchNamingWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BranchNamingWindowControl"/> class.
        /// </summary>
        public BranchNamingWindowControl()
        {
            this.InitializeComponent();
        }

        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button_Click(object sender, RoutedEventArgs e)
        {

            if (!string.IsNullOrEmpty(txbTaskTitle.Text))
            {
                var branchName = "";

                lblSymbolsCount.Content = $"{txbTaskTitle.Text.Length} symbols";
                lblSymbolsCount.Foreground = txbTaskTitle.Text.Length > 72 ? Brushes.Red : Brushes.Black;
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

                var taskTitle = textInfo.ToTitleCase(txbTaskTitle.Text).Replace(" ", "");
                int charLocation = taskTitle.IndexOf("(", StringComparison.Ordinal);
                var issueNumber = new String(taskTitle.ToCharArray().Where(c => Char.IsDigit(c)).ToArray());
                string issueId = taskTitle.Substring(charLocation + 1, taskTitle.IndexOf("-", StringComparison.Ordinal) - (charLocation + 1)).ToLower();

                if (cbBug.IsChecked.HasValue && cbBug.IsChecked.Value)
                {
                    branchName = $"bug-{cmbxBranches.SelectedValue}-{issueId}-{issueNumber}-{taskTitle.Substring(0, charLocation)}";
                }

                if (cbTask.IsChecked.HasValue && cbTask.IsChecked.Value)
                {
                    branchName = $"{cmbxBranches.SelectedValue}-{issueId}-{issueNumber}-{taskTitle.Substring(0, charLocation)}";
                }

                if (cbFeature.IsChecked.HasValue && cbFeature.IsChecked.Value)
                {
                    branchName = $"{issueId}-{issueNumber}-feature-{taskTitle.Substring(0, charLocation)}";
                }

                if (cbFeatureFrom.IsChecked.HasValue && cbFeatureFrom.IsChecked.Value)
                {
                    branchName = $"feature-{cmbxBranches.SelectedValue}-{taskTitle.Substring(0, charLocation)}";
                }

                txbBranchTitle.Text = branchName;

            }

        }

        private void Loadbranches()
        {
            cmbxBranches.Items.Clear();

            try
            {
                var dte2 = (DTE2)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SDTE));
                if (!string.IsNullOrEmpty(dte2?.Solution?.FullName))
                {
                    string solutionDir = System.IO.Path.GetDirectoryName(dte2.Solution.FullName);

                    string cmd = $"/c cd {solutionDir} & git branch";
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    proc.StartInfo.FileName = "cmd.exe";
                    proc.StartInfo.Arguments = cmd;
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.Start();

                    var branchNames = proc.StandardOutput.ReadToEnd();
                    var branches = branchNames.Split(new[] { "\n" }, StringSplitOptions.None).ToList();
                    
                    cmbxBranches.DisplayMemberPath = "Text";
                    cmbxBranches.SelectedValuePath = "Value";
                    var currentBranch = "";

                    foreach (var branch in branches)
                    {
                        var name = branch.TrimStart('*').Trim();

                        if (!string.IsNullOrEmpty(name))
                        {
                            cmbxBranches.Items.Add(new ComboBoxItem { Text = name, Value = name });

                            if (branch.StartsWith("*"))
                            {
                                currentBranch = name;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(currentBranch))
                    {
                        cmbxBranches.SelectedValue = currentBranch;
                    }
                }
            }

            catch { }
        }

        private void cmbxBranches_Loaded(object sender, RoutedEventArgs e)
        {
            Loadbranches();
        }
    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
    }
}