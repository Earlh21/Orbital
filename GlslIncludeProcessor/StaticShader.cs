using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace GlslIncludeProcessor
{
	public static class StaticShader
	{
		public static void ProcessShader(String file)
		{
			String dir = Path.GetDirectoryName(file);
			
			string[] lines_array = File.ReadAllLines(file);
			List<String> lines = lines_array.ToList();

			List<String> includes = new List<string>();
			for (int i = 0; i < lines.Count; i++)
			{
				if (lines[i].StartsWith("#include"))
				{
					string[] args = lines[i].Split(' ');
					includes.Add(dir + "\\" + args[1]);
									
					lines.RemoveAt(i);
					i--;
				}
			}

			if (includes.Count == 0)
			{
				return;
			}
			
			lines.Insert(1, String.Empty);
			for(int i = includes.Count - 1; i >= 0; i--)
			{
				String[] include_lines = File.ReadAllLines(includes[i]);
	
				lines.Insert(2, String.Empty);			
				for (int j = include_lines.Length - 1; j >= 0; j--)
				{
					lines.Insert(2, include_lines[j]);
				}
			}

			String contents = String.Join(Environment.NewLine, lines);
			File.WriteAllText(file, contents);
		}
	}
}