using System;

namespace GarageManagement.Shared;

public static class DbConstraintExceptionHelper
{
    public static bool ContainsConstraintName(Exception ex, string constraintName)
    {
        if (ex.Message.Contains(constraintName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ex.InnerException != null && ContainsConstraintName(ex.InnerException, constraintName);
    }
}
