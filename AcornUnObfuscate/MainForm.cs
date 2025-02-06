using System;
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
        private BasicSyntaxHighlighter syntaxHighlighter;

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

            //new menu for help / about
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");
            ToolStripMenuItem aboutMenuItem = new ToolStripMenuItem("About", null, (s, e) => MessageBox.Show("Acorn BASIC Deobfuscator\n\nThis tool is designed to help deobfuscate Acorn BASIC code.\n\n(c) 2025 Aidan Hutchinson\n\nContributors: Aidan Hutchinson\n\nhttps://github.com/aidanhutch/AcornUnObfuscate", "About", MessageBoxButtons.OK, MessageBoxIcon.Information));
            helpMenu.DropDownItems.Add(aboutMenuItem);
            menuStrip.Items.Add(helpMenu);

            // Setup RichTextBox
            richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new System.Drawing.Font("Consolas", 10F),
                WordWrap = false,
                ForeColor = Color.FromArgb(220, 220, 220),
                BackColor = Color.FromArgb(30, 30, 30),
                DetectUrls = false
            };

            // Form setup
            
            Controls.Add(richTextBox);
            Controls.Add(menuStrip);
            Text = "Acorn BASIC Deobfuscator";
            Size = new System.Drawing.Size(800, 600);

            syntaxHighlighter = new BasicSyntaxHighlighter(richTextBox);
        }

        private void RevertChanges(object? sender, EventArgs e)
        {
            richTextBox.Visible = false;
            richTextBox.Clear();
            richTextBox.Lines = previousLines;
            richTextBox.SelectionStart = 0;
            richTextBox.Visible = true;
            syntaxHighlighter.HighlightSyntax();
        }

        private void DeObfuscate(object sender, EventArgs e)
        {
            richTextBox.Visible = false;
            var text = richTextBox.Lines.ToList();
            previousLines = richTextBox.Lines;
            var converted = deobfuscator.DeobfuscateCode(text);
            richTextBox.Clear();
           
            foreach (var line in converted)
            {
                richTextBox.AppendText($"{line}\n");
            }
            richTextBox.SelectionStart = 0;
            
            syntaxHighlighter.HighlightSyntax();
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
                        syntaxHighlighter.HighlightSyntax();
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
