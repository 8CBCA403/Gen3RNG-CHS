using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace _3genSearch;

internal class DataGridViewWrapper<TData>
{
	private readonly DataGridView _dataGridView;

	private readonly Dictionary<string, DataGridViewTextBoxColumn> _columnMap;

	public DataGridViewWrapper(DataGridView dataGridView)
	{
		_dataGridView = dataGridView;
		_columnMap = new Dictionary<string, DataGridViewTextBoxColumn>();
		PropertyInfo property = typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
		property.SetValue(dataGridView, true, null);
		List<DataGridViewTextBoxColumn> list = new List<DataGridViewTextBoxColumn>();
		PropertyInfo[] properties = typeof(TData).GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			object[] customAttributes = propertyInfo.GetCustomAttributes(inherit: true);
			foreach (object obj in customAttributes)
			{
				if (obj is DataGridViewRowHeaderAttribute dataGridViewRowHeaderAttribute)
				{
					DataGridViewTextBoxColumn dataGridViewTextBoxColumn = new DataGridViewTextBoxColumn
					{
						DataPropertyName = propertyInfo.Name,
						HeaderText = (dataGridViewRowHeaderAttribute.HeaderText ?? propertyInfo.Name),
						ReadOnly = true,
						Resizable = DataGridViewTriState.False,
						Width = dataGridViewRowHeaderAttribute.Width,
						MinimumWidth = dataGridViewRowHeaderAttribute.Width,
						SortMode = DataGridViewColumnSortMode.Programmatic
					};
					if (dataGridViewRowHeaderAttribute.Resizable)
					{
						dataGridViewTextBoxColumn.Resizable = DataGridViewTriState.True;
					}
					if (dataGridViewRowHeaderAttribute.Font != null)
					{
						dataGridViewTextBoxColumn.DefaultCellStyle.Font = new Font(dataGridViewRowHeaderAttribute.Font, 9f);
					}
					list.Add(dataGridViewTextBoxColumn);
					_columnMap.Add(propertyInfo.Name, dataGridViewTextBoxColumn);
				}
			}
		}
		DataGridViewColumnCollection columns = dataGridView.Columns;
		DataGridViewColumn[] dataGridViewColumns = list.ToArray();
		columns.AddRange(dataGridViewColumns);
		dataGridView.AutoGenerateColumns = false;
	}

	public void SetData(IEnumerable<TData> data)
	{
		_dataGridView.DataSource = data.ToArray();
	}

	public void SetData(IList<TData> data)
	{
		_dataGridView.DataSource = data;
	}

	public void SetColumnVisible(string key, bool value)
	{
		_columnMap[key].Visible = value;
	}
}
