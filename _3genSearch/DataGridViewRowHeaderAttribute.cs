using System;

namespace _3genSearch;

internal class DataGridViewRowHeaderAttribute : Attribute
{
	public int Width { get; }

	public string HeaderText { get; }

	public bool Resizable { get; }

	public string Font { get; }

	public DataGridViewRowHeaderAttribute(int width, string headerText, bool resizable = false, string font = null)
	{
		bool flag = resizable;
		Width = width;
		HeaderText = headerText;
		Resizable = flag;
		Font = font;
	}
}
