namespace SimpleRESTApplication.Models
{
    public class InscriptionData
    {
        public string id { get; set; }
        public string pos { get; set; }
        public string name { get; set; }
    }

    public class InscriptionPlusSignature
    {
        public InscriptionData data { get; set; }
        public string signature { get; set; }
    }
}