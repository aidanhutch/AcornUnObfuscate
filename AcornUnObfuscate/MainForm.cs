﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AcornUnObfuscate;

namespace AcornUnOfuscate
{
    public partial class MainForm : Form
    {
        private BasicDetokenizer detokenizer;
        private RichTextBox richTextBox;
        private BasicDeobfuscator deobfuscator;
        private string[] previousLines;

        public MainForm()
        {
            InitializeComponent();
            detokenizer = new BasicDetokenizer();
            deobfuscator = new BasicDeobfuscator();
            SetupForm();
        }

        private void SetupForm()
        {
            //set the background color to a really dark grey rgb
            
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Setup MenuStrip
            MenuStrip menuStrip = new MenuStrip();
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("File");
            ToolStripMenuItem openMenuItem = new ToolStripMenuItem("Open", null, OpenFile);
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());

            fileMenu.DropDownItems.Add(openMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);
            menuStrip.Items.Add(fileMenu);

            //new menu for deobfuscation
            ToolStripMenuItem deobfuscateMenu = new ToolStripMenuItem("Deobfuscate");
            ToolStripMenuItem deobfuscateMenuItem = new ToolStripMenuItem("Deobfuscate", null, DeObfuscate);
            ToolStripMenuItem deobfuscateMenuItem2 = new ToolStripMenuItem("Revert Changes", null, RevertChanges);
            deobfuscateMenu.DropDownItems.Add(deobfuscateMenuItem);
            deobfuscateMenu.DropDownItems.Add(deobfuscateMenuItem2);
            menuStrip.Items.Add(deobfuscateMenu);


            // Setup RichTextBox
            richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 12F),
                WordWrap = false,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            // Form setup
            
            Controls.Add(richTextBox);
            Controls.Add(menuStrip);
            Text = "Acorn BASIC Detokenizer";
            Size = new System.Drawing.Size(800, 600);
        }

        private void RevertChanges(object? sender, EventArgs e)
        {
            richTextBox.Visible = false;
            richTextBox.Clear();
            richTextBox.Lines = previousLines;
            richTextBox.SelectionStart = 0;
            richTextBox.Visible = true;
        }

        private void DeObfuscate(object sender, EventArgs e)
        {
            var text = richTextBox.Lines.ToList();
            previousLines = richTextBox.Lines;
            var converted = deobfuscator.DeobfuscateCode(text);
            richTextBox.Clear();
            richTextBox.Visible = false;
            foreach (var line in converted)
            {
                richTextBox.AppendText($"{line}\n");
            }
            richTextBox.SelectionStart = 0;
            richTextBox.Visible = true;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "BBC BASIC files (*.bas)|*.bas|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lines = detokenizer.DetokenizeFile(openFileDialog.FileName);
                        richTextBox.Clear();

                        richTextBox.Visible = false;
                        foreach (var line in lines)
                        {
                            richTextBox.AppendText($"{line.LineNumber} {line.Content}\n");
                        }

                        // Scroll to top
                        richTextBox.SelectionStart = 0;
                        richTextBox.Visible = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        
    }
}
