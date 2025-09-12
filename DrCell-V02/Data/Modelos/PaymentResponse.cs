namespace DrCell_V02.Data.Modelos
{
    public class PaymentResponse
    {
        public string collection_id { get; set; } = string.Empty;
        public string collection_status { get; set; } = string.Empty;
        public string payment_id { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string external_reference { get; set; } = string.Empty;
        public string payment_type { get; set; } = string.Empty;
        public string merchant_order_id { get; set; } = string.Empty;
        public string preference_id { get; set; } = string.Empty;
        public string site_id { get; set; } = string.Empty;
        public string processing_mode { get; set; } = string.Empty;
        public string merchant_account_id { get; set; } = string.Empty;
    }
}
