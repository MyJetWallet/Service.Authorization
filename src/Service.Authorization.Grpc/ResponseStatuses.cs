namespace Service.Authorization.Grpc
{
    public enum ResponseStatuses
    {
        Successful,
        SystemError,
        ClientAlreadyExists,        
        CountryRestricted

    }
}