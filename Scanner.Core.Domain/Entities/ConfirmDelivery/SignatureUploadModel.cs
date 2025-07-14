namespace Scanner.Core.Domain.Entities.ConfirmDelivery
{
    public class SignatureUploadModel
    {
        public int RunnNo { get; set; }
        public string CustomerName { get; set; }
        public string SignatureFileURL { get; set; }

        public string ImageFileURL { get; set; }

    }
}
