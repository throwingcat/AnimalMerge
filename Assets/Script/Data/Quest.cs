using Violet;
 using System;
namespace SheetData
{
 public partial class Quest : CSVDataBase 
{ 
	public string Condition { get; set; }

	public string Description { get; set; }

	public int Count { get; set; }

	public int Point { get; set; }

	public int Coin { get; set; }

	public string TrackerKey { get; set; }

}
}
