namespace ApiControllersInDotNet
{
    public class Payment
    {
        public readonly string Description;
        
        private Payment(string description) => Description = description;

        public static Payment FromDetailedSource(ShallowPayment shallowPayment, DetailedPayment detailedPayment) => 
            new Payment("Super detailed payment details!");

        public static Payment FromShallowSource(ShallowPayment shallowDetailedPaymentTask) => 
            new Payment("Just the bare essentials, sorry.");
    }
}