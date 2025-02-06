using AcornUnOfuscate;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace AcornUnObfuscate
{
    public partial class MainForm : Form
    {
        private readonly BasicDetokenizer detokenizer;
        private RichTextBox _richTextBox;
        private readonly BasicDeobfuscator deobfuscator;
        private string[] _previousLines;
        private BasicSyntaxHighlighter _syntaxHighlighter;

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
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            var openMenuItem = new ToolStripMenuItem("Open", null, OpenFile);
            var exitMenuItem = new ToolStripMenuItem("Exit", null, (s, e) => Close());

            fileMenu.DropDownItems.Add(openMenuItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add(exitMenuItem);
            menuStrip.Items.Add(fileMenu);

            //new menu for deobfuscation
            var deobfuscateMenu = new ToolStripMenuItem("Deobfuscate");
            var deobfuscateMenuItem = new ToolStripMenuItem("Deobfuscate", null, DeObfuscate);
            var deobfuscateMenuItem2 = new ToolStripMenuItem("Revert Changes", null, RevertChanges);
            deobfuscateMenu.DropDownItems.Add(deobfuscateMenuItem);
            deobfuscateMenu.DropDownItems.Add(deobfuscateMenuItem2);
            menuStrip.Items.Add(deobfuscateMenu);

            //new menu for help / about
            var helpMenu = new ToolStripMenuItem("Help");
            var aboutMenuItem = new ToolStripMenuItem("About", null, (s, e) => MessageBox.Show("Acorn BASIC Deobfuscator\n\nThis tool is designed to help deobfuscate Acorn BASIC code.\n\n(c) 2025 Aidan Hutchinson\n\nContributors: Aidan Hutchinson\n\nhttps://github.com/aidanhutch/AcornUnObfuscate", "About", MessageBoxButtons.OK, MessageBoxIcon.Information));
            helpMenu.DropDownItems.Add(aboutMenuItem);
            menuStrip.Items.Add(helpMenu);

            // Setup RichTextBox
            _richTextBox = new RichTextBox
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
            
            Controls.Add(_richTextBox);
            Controls.Add(menuStrip);
            Text = "Acorn BASIC Deobfuscator";
            Size = new System.Drawing.Size(800, 600);

            _syntaxHighlighter = new BasicSyntaxHighlighter(_richTextBox);
        }

        private void RevertChanges(object? sender, EventArgs e)
        {
            _richTextBox.Visible = false;
            _richTextBox.Clear();
            _richTextBox.Lines = _previousLines;
            _richTextBox.SelectionStart = 0;
            _richTextBox.Visible = true;
            _syntaxHighlighter.HighlightSyntax();
        }

        private void DeObfuscate(object sender, EventArgs e)
        {
            _richTextBox.Visible = false;
            var text = _richTextBox.Lines.ToList();
            _previousLines = _richTextBox.Lines;
            var converted = deobfuscator.DeobfuscateCode(text);
            _richTextBox.Clear();
           
            foreach (var line in converted)
            {
                _richTextBox.AppendText($"{line}\n");
            }
            _richTextBox.SelectionStart = 0;
            
            _syntaxHighlighter.HighlightSyntax();
            _richTextBox.Visible = true;
        }

        private void OpenFile(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "BBC BASIC files (*.bas)|*.bas|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var lines = detokenizer.DetokenizeFile(openFileDialog.FileName);
                        _richTextBox.Clear();

                        _richTextBox.Visible = false;
                        foreach (var line in lines)
                        {
                            _richTextBox.AppendText($"{line.LineNumber} {line.Content}\n");
                        }

                        // Scroll to top
                        _richTextBox.SelectionStart = 0;
                        _syntaxHighlighter.HighlightSyntax();
                        _richTextBox.Visible = true;
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
