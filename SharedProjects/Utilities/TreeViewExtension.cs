using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;

namespace Utilities
{
    public static class TreeViewExtension
    {
        public static string ToPrettyString(this TreeView tree, bool ignoreHiddenNodes = true)
        {
            string result = "";
            foreach (TreeNode node in tree.Nodes)
            {
                result += node.ToPrettyString();
                result += "\r\n";
            }
            return result;
        }

        public static string ToPrettyString(this TreeNode node, bool ignoreHiddenNodes = true)
        {
            string result = "";
            if (ignoreHiddenNodes && node.Parent != null && !node.Parent.IsExpanded)
            {
                return result;
            }

            for (int i = 0; i < node.Level; i++)
            {
                result += "\t";
            }
            result += node.Text;
            foreach (TreeNode childNode in node.Nodes)
            {
                var childString = childNode.ToPrettyString();
                if (childString != string.Empty)
                {
                    result += "\r\n";
                    result += childString;
                }
            }
            return result;
        }
    }
}
