namespace BillWise.Models.Services
{
    public record IconGroup(string GroupName, string[] Icons);

    public static class IconLibrary
    {
        public static readonly IconGroup[] Groups =
        {
            new("🏠 Home", new[]
            {
                "🏠", "🏡", "🛋️", "🪑", "🛏️", "🚿", "💡", "🔌", "🧹", "🪴", "🔑", "🪟"
            }),
            new("🍕 Food & Drink", new[]
            {
                "🍕", "🍔", "🍜", "🍣", "☕", "🧃", "🛒", "🥗", "🍰", "🥩", "🍺", "🫖"
            }),
            new("🚗 Transport", new[]
            {
                "🚗", "✈️", "🚌", "🚇", "⛽", "🛵", "🚕", "🚂", "🚢", "🚲", "🛻", "🚁"
            }),
            new("💪 Sport & Health", new[]
            {
                "💊", "🏥", "🦷", "💪", "🏋️", "⚽", "🏀", "🎾", "🏊", "🧘", "🚴", "🏃"
            }),
            new("🎮 Entertainment", new[]
            {
                "🎮", "🎬", "🎵", "📺", "🎯", "🎲", "🃏", "🎸", "🎪", "🍿", "🎭", "🕹️"
            }),
            new("💼 Work & Finance", new[]
            {
                "💼", "💻", "📱", "🖨️", "📊", "💰", "💳", "🏦", "📝", "📋", "🖊️", "📁"
            }),
            new("🎓 Education", new[]
            {
                "🎓", "📚", "✏️", "📐", "🔬", "🧪", "📖", "🏫", "🎒", "🗺️", "🖍️", "🔭"
            }),
            new("🌟 Other", new[]
            {
                "🐕", "🐈", "🌺", "🌍", "⛺", "🎁", "🧴", "💇", "💈", "💅", "🧺", "⚙️"
            }),
        };
    }
}
