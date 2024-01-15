namespace Identity.Exceptions;

public class KeycloakException: Exception
{
    public KeycloakException():base()
    {
        
    }
    public KeycloakException(string o):base(o)
    {
        
    }
}