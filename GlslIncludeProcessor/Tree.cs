using System.Collections.Generic;

namespace GlslIncludeProcessor
{
	public class Tree<T>
	{
		public TreeNode Root;

		public TreeNode AddChild(TreeNode parent, string value)
		{
			
		}
		

		public class TreeNode
		{
			private TreeNode parent;
			private List<TreeNode> children;

			public T value;

			public TreeNode(TreeNode parent, string value)
			{
				this.parent = parent;
				this.value = value;
			}
		}
	}
}