using System.Collections.Generic;

namespace GlslIncludeProcessor
{
	public class TreeNode<T>
	{
		public TreeNode<T> Parent { get; internal set; }
		internal List<TreeNode<T>> children;

		public T Value { get; set; }

		public TreeNode(TreeNode<T> parent, T value)
		{
			children = new List<TreeNode<T>>();
			Parent = parent;
			Value = value;
		}
	}
}