namespace Beerbox.Service.Contracts;

public static class PrefectureExtensions
{
	private static readonly Dictionary<Prefecture, (string English, string Japanese)> labels;

	static PrefectureExtensions()
	{
		var data = new (Prefecture prefecture, string English, string Japanese)[]
		{
			(Prefecture.Hokkaido, "Hokkaido", "北海道"),
			(Prefecture.Aomori, "Aomori-ken", "青森県"),
			(Prefecture.Iwate, "Iwate-ken", "岩手県"),
			(Prefecture.Miyagi, "Miyagi-ken", "宮城県"),
			(Prefecture.Akita, "Akita-ken", "秋田県"),
			(Prefecture.Yamagata, "Yamagata-ken", "山形県"),
			(Prefecture.Fukushima, "Fukushima-ken", "福島県"),
			(Prefecture.Ibaraki, "Ibaraki-ken", "茨城県"),
			(Prefecture.Tochigi, "Tochigi-ken", "栃木県"),
			(Prefecture.Gunma, "Gunma-ken", "群馬県"),
			(Prefecture.Saitama, "Saitama-ken", "埼玉県"),
			(Prefecture.Chiba, "Chiba-ken", "千葉県"),
			(Prefecture.Tokyo, "Tokyo-to", "東京都"),
			(Prefecture.Kanagawa, "Kanagawa-ken", "神奈川県"),
			(Prefecture.Niigata, "Niigata-ken", "新潟県"),
			(Prefecture.Toyama, "Toyama-ken", "富山県"),
			(Prefecture.Ishikawa, "Ishikawa-ken", "石川県"),
			(Prefecture.Fukui, "Fukui-ken", "福井県"),
			(Prefecture.Yamanashi, "Yamanashi-ken", "山梨県"),
			(Prefecture.Nagano, "Nagano-ken", "長野県"),
			(Prefecture.Gifu, "Gifu-ken", "岐阜県"),
			(Prefecture.Shizuoka, "Shizuoka-ken", "静岡県"),
			(Prefecture.Aichi, "Aichi-ken", "愛知県"),
			(Prefecture.Mie, "Mie-ken", "三重県"),
			(Prefecture.Shiga, "Shiga-ken", "滋賀県"),
			(Prefecture.Kyoto, "Kyoto-fu", "京都府"),
			(Prefecture.Osaka, "Osaka-fu", "大阪府"),
			(Prefecture.Hyogo, "Hyogo-ken", "兵庫県"),
			(Prefecture.Nara, "Nara-ken", "奈良県"),
			(Prefecture.Wakayama, "Wakayama-ken", "和歌山県"),
			(Prefecture.Tottori, "Tottori-ken", "鳥取県"),
			(Prefecture.Shimane, "Shimane-ken", "島根県"),
			(Prefecture.Okayama, "Okayama-ken", "岡山県"),
			(Prefecture.Hiroshima, "Hiroshima-ken", "広島県"),
			(Prefecture.Yamaguchi, "Yamaguchi-ken", "山口県"),
			(Prefecture.Tokushima, "Tokushima-ken", "徳島県"),
			(Prefecture.Kagawa, "Kagawa-ken", "香川県"),
			(Prefecture.Ehime, "Ehime-ken", "愛媛県"),
			(Prefecture.Kochi, "Kochi-ken", "高知県"),
			(Prefecture.Fukuoka, "Fukuoka-ken", "福岡県"),
			(Prefecture.Saga, "Saga-ken", "佐賀県"),
			(Prefecture.Nagasaki, "Nagasaki-ken", "長崎県"),
			(Prefecture.Kumamoto, "Kumamoto-ken", "熊本県"),
			(Prefecture.Oita, "Oita-ken", "大分県"),
			(Prefecture.Miyazaki, "Miyazaki-ken", "宮崎県"),
			(Prefecture.Kagoshima, "Kagoshima-ken", "鹿児島県"),
			(Prefecture.Okinawa, "Okinawa-ken", "沖縄県"),
		};
		labels = data.ToDictionary(static item => item.prefecture, static item => (item.English, item.Japanese));
	}

	public static bool TryGetPrefecture(string name, out Prefecture prefecture)
	{
		prefecture = labels
			.FirstOrDefault(item =>
				item.Key.ToString() == name || item.Value.English == name || item.Value.Japanese == name
			)
			.Key;
		return prefecture != default;
	}

	public static Prefecture GetPrefecture(string name) =>
		TryGetPrefecture(name, out var prefecture) ? prefecture : throw new ArgumentOutOfRangeException(nameof(name));

	public static string GetLabel(this Prefecture prefecture, Language language) =>
		language == Language.Japanese ? labels[prefecture].Japanese : labels[prefecture].English;
}
