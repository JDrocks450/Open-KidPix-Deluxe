namespace KidPixRevamped
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            ResourceTree = new TreeView();
            previewBox = new TextBox();
            SuspendLayout();
            // 
            // ResourceTree
            // 
            ResourceTree.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            ResourceTree.Location = new Point(12, 12);
            ResourceTree.Name = "ResourceTree";
            ResourceTree.Size = new Size(336, 697);
            ResourceTree.TabIndex = 0;
            ResourceTree.AfterSelect += ResourceTree_AfterSelect;
            // 
            // previewBox
            // 
            previewBox.Location = new Point(354, 12);
            previewBox.Multiline = true;
            previewBox.Name = "previewBox";
            previewBox.Size = new Size(640, 298);
            previewBox.TabIndex = 1;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1006, 721);
            Controls.Add(previewBox);
            Controls.Add(ResourceTree);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView ResourceTree;
        private TextBox previewBox;
    }
}
