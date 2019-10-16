using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GlslIncludeProcessor
{
	public class Shader
	{
		private DependencyTree<String> dependency;
		private string file;

		public Shader(string file)
		{
			bool is_lib = false;
			
			using (StreamReader reader = new StreamReader(file))
			{
				while (true)
				{
					string line = reader.ReadLine();

					if (String.IsNullOrWhiteSpace(line))
					{
						if (line == null)
						{
							break;
						}

						continue;
					}

					if (line.Equals("#lib"))
					{
						is_lib = true;
						break;
					}
				}
			}

			if (!is_lib)
			{
				dependency = new DependencyTree<string>();			
				dependency.Root = new TreeNode<string>(null, file);
				AddAllDependencies(dependency.Root);
			}

			this.file = file;
		}
		
		private List<String> FindDependencies(string file)
		{
			string[] lines = File.ReadAllLines(file);

			List<string> includes = new List<string>();
			foreach (string line in lines)
			{
				if (line.StartsWith("#include"))
				{
					string[] args = line.Split(' ');
					includes.Add(args[1]);
				}
			}

			return includes;
		}
		
		private void AddAllDependencies(TreeNode<string> node)
		{
			List<string> includes = FindDependencies(node.Value);

			foreach (string include in includes)
			{
				TreeNode<string> child = dependency.AddChild(node, include);
				AddAllDependencies(child);
			}
		}

		private void InsertDependency(List<string> main, string include)
		{
			string[] include_lines = File.ReadAllLines(include);
			
			main.Insert(2, Environment.NewLine);
			for (int i = include_lines.Length - 1; i >= 0; i--)
			{
				main.Insert(2, include_lines[i]);
			}
		}

		public string Process()
		{
			List<string> lines = File.ReadAllLines(file).ToList();

			if (dependency == null)
			{
				//Get rid of the #lib tag
				lines.RemoveAt(0);
				return String.Join(Environment.NewLine, lines);
			}

			foreach (string include in dependency.PreOrder())
			{
				InsertDependency(lines, include);
			}

			return String.Join(Environment.NewLine, lines);
		}
	}
}