namespace _Game.Scripts.Core.Data
{
    // Các loại tài nguyên trong game
    public enum ResourceType
    {
        Coin,   // Tiền (Score) - Dùng để trả nợ và mua lượt quay
        Ticket, // Vé (Token) - Dùng để mua Charm/Item trong Shop
        Debt    // Nợ (Mục tiêu cần đạt được)
    }

    // Các trạng thái của vòng lặp Game
    public enum GameState
    {
        Preparation, // Đang ở sảnh/Shop: Mua Charm, chọn gói Spin
        Spinning,    // Đang trong quá trình quay (chờ hết số lượt)
        RoundEnd,    // Kết thúc vòng: Tổng kết nợ
        GameOver,    // Không trả được nợ
        Victory      // (Tuỳ chọn) Thắng game
    }
}