namespace BulkyBook.Utility
{
    public static class SD
    {
        public static class StoreProcedureName
        {
            public const string Proc_CoverType_GetAll = "usp_GetCoverTypes";
            public const string Proc_CoverType_Get = "usp_GetCoverType";
            public const string Proc_CoverType_Update = "usp_UpdateCoverType";
            public const string Proc_CoverType_Delete = "usp_DeleteCoverType";
            public const string Proc_CoverType_Create = "usp_CreateCoverType";
        }

        public static class Roles
        {
            public const string IndividualCustomer = "IndividualCustomer";
            public const string CompanyCustomer = "CompanyCustomer";
            public const string Admin = "Admin";
            public const string Employee = "Employee";
        }

        public static class Constants
        {
            public const string ShoppingCartSession = "ShoppingCartSession";
        }

        public static class OrderStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Processing = "Processing";
            public const string Shipped = "Shipped";
            public const string Cancelled = "Cancelled";
            public const string Refunded = "Refunded";
        }

        public static class PaymentStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string ApprovedForDelayedPayment = "ApprovedForDelayedPayment";
            public const string Rejected = "Rejected";
        }

        public static double GetPriceBasedOnQuantity(double quantity, double price, double price50, double price100)
        {
            if (quantity < 50)
                return price;
            else
            {
                if (quantity < 100)
                    return price50;
                else
                    return price100;
            }
        }

        public static string ConvertToRawHtml(string source)
        {
            char[] array = new char[source.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < source.Length; i++)
            {
                char let = source[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);
        }
    }
}
