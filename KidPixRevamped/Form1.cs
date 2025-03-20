using KidPix.API.Importer.Mohawk;

namespace KidPixRevamped
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var file = MHWKImporter.Import(@"C:\Program Files (x86)\The Learning Company\Kid Pix Deluxe 4\Data\EnterName.MHK");

            ResourceTree.Nodes.Clear();
            foreach (var type in file.Resources)
            {
                var typeNode = ResourceTree.Nodes.Add(type.Key.ToString());
                foreach (ResourceTableEntry resource in type.Value)
                {
                    TreeNode resNode = typeNode.Nodes.Add(string.IsNullOrWhiteSpace(resource.Name) ? resource.Id.ToString() : resource.Name);
                    resNode.Tag = resource;
                }
            }
        }

        private void ResourceTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is ResourceTableEntry entry)
                previewBox.Text = $"Name: {entry.Name} \nOffset: {entry.Offset:X8} \nID: {entry.Id} \nIndex: {entry.Index} \nSize: {entry.Size}";
        }
    }
}
