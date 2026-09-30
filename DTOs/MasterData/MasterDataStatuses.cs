namespace TradePlatform.Api.DTOs.MasterData;

public class MasterDataStatuses : MasterDataStatusMeta
{
	public int id { get; set; }

	public string code { get; set; }

	public string name { get; set; }

	public string description { get; set; }

	public int sort_order { get; set; }

	public new bool is_active { get; set; }
}
