using Violet;
 using System;
namespace SheetData
{
 public partial class Unit : CSVDataBase 
{ 
	public string master { get; set; }

	public bool isPlayerUnit { get; set; }

	public int index { get; set; }

	public string grow_unit { get; set; }

	public double size { get; set; }

	public int score { get; set; }

	public string face_texture { get; set; }

	public string piece_texture { get; set; }

	public string name { get; set; }

}
}
