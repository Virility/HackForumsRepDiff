﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using HackForumsRepDiff.Core.Helpers;
using HackForumsRepDiff.Core.Models;
using HackForumsRepDiff.UI.Controls;

namespace HackForumsRepDiff.UI.Forms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void tsmiAdd_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog
            {
                RootFolder = Environment.SpecialFolder.Desktop
            })
            {
                if (dialog.ShowDialog() != DialogResult.OK)
                    return;

                var files = Directory.GetFiles(dialog.SelectedPath, "*", SearchOption.AllDirectories).
                    Where(f => Config.AllowedFileTypes.Any(f.EndsWith)).ToList();

                files.Sort(new NumericComparer());
                InvalidateReputationsFromFiles(files);
            }
        }

        private void tsmiRemove_Click(object sender, EventArgs e)
        {
            foreach (PageViewItem item in lvLoadedDocuments.SelectedItems)
                lvLoadedDocuments.Items.Remove(item);

            var files = lvLoadedDocuments.Items.Cast<PageViewItem>().Select(p => p.Text);
            InvalidateReputationsFromFiles(files.ToArray());
        }

        public void InvalidateReputationsFromFiles(IEnumerable<string> files)
        {
            lvLoadedDocuments.Items.Clear();
            lbTotalFiles.Text = TotalFormatter.FormatTotalFiles(0);
            rcGiven.Clear();
            rcReceived.Clear();
            rcDifference.Clear();

            var parsed = files.
                Select(f => PageParser.Parse(f, PageReadType.FromFile));

            foreach (var page in parsed)
            {
                if (page.Type == TransactionType.Unknown)
                    continue;

                lvLoadedDocuments.Items.Add(page.ToPageViewItem());

                switch (page.Type)
                {
                    case TransactionType.Given:
                        rcGiven.AddRange(page.Reputations);
                        break;
                    case TransactionType.Received:
                        rcReceived.AddRange(page.Reputations);
                        break;
                }
            }

            rcDifference.AddRange(Differentiator.Differenciate(rcGiven.Reputations, rcReceived.Reputations).ToArray());
            lbTotalFiles.Text = TotalFormatter.FormatTotalFiles(lvLoadedDocuments.Items.Count);
        }
    }
}