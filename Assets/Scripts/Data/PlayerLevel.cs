using Violet;
 using System;
namespace SheetData
{
 public partial class PlayerLevel : CSVDataBase 
{ 
	public int level { get; set; }

	public int exp { get; set; }

	public string reward { get; set; }

	public int amount { get; set; }

	public string premium_reward { get; set; }

	public int premium_amount { get; set; }

}
}
