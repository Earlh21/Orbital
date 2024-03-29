using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GlslIncludeProcessor
{
	public class Shader
	{
		private DependencyTree<String> dependency;
		private string filename;

		public Shader(string filename)
		{
			bool is_lib = false;
			
			using (StreamReader reader = new StreamReader(filename))
			{
				string line;
				
				while (reader.Peek() != -1)
				{
					line = reader.ReadLine();

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
				dependency.Root = new TreeNode<string>(null, filename);
				AddAllDependencies(dependency.Root);
			}

			this.filename = filename;
		}
		
		private List<String> FindDependencies(string file)
		{
			String dir = Path.GetDirectoryName(file);
			string[] lines = File.ReadAllLines(file);

			List<string> includes = new List<string>();
			foreach (string line in lines)
			{
				if (line.StartsWith("#include"))
				{
					string[] args = line.Split(' ');
					includes.Add(dir + "\\" + args[1]);
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

				if (child != null)
				{
					AddAllDependencies(child);
				}
			}
		}

		private void InsertDependency(List<string> main, string include)
		{
			string[] include_lines = File.ReadAllLines(include);
			
			main.Insert(2, Environment.NewLine);
			for (int i = include_lines.Length - 1; i >= 0; i--)
			{
				if (include_lines[i].StartsWith("#"))
				{
					continue;
				}
				
				main.Insert(2, include_lines[i]);
			}
		}

		private void RemoveDirectives(List<String> lines)
		{
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].StartsWith("#include"))
				{
					lines.RemoveAt(i);
					i--;
				}
			}
		}

		public string Process()
		{
			List<string> lines = File.ReadAllLines(filename).ToList();

			if (dependency == null)
			{
				return String.Join(Environment.NewLine, lines);
			}

			List<string> traversal = dependency.Traversal();
			traversal.Reverse();
			foreach (string include in traversal)
			{
				if (include.Equals(dependency.Root.Value))
				{
					continue;
				}
				
				InsertDependency(lines, include);
			}
			
			RemoveDirectives(lines);

			return String.Join(Environment.NewLine, lines);
		}
	}
}