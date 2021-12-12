using Violet;
 using System;
namespace SheetData
{
 public partial class UnitLevel : CSVDataBase 
{ 
	public string group { get; set; }

	public int exp { get; set; }

	public int total { get; set; }

	public double upgrade_ratio { get; set; }

	public bool max { get; set; }

	public string comment { get; set; }

}
}
