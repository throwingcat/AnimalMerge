using Violet;
 using System;
namespace SheetData
{
 public partial class Achievement : CSVDataBase 
{ 
	public string Comment { get; set; }

	public string Description { get; set; }

	public string TrackerKey { get; set; }

	public string SubKey { get; set; }

	public bool Loop { get; set; }

	public int Grow { get; set; }

	public string Reward1 { get; set; }

	public int Amount1 { get; set; }

	public string Reward2 { get; set; }

	public int Amount2 { get; set; }

}
}
