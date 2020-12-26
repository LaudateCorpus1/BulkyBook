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
    }
}
