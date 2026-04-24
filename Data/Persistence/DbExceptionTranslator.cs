using FlagForge.Data.Constants;
using FlagForge.Data.Exceptions;
using FlagForge.Data.Persistence.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Persistence;

public class DbExceptionTranslator : IDbExceptionTranslator
{
    private const string TableNameKey = "TableName";
    private const string SerializationFailureCode = "40001";
    private const string QueryCancelledCode = "57014";
    private const string UniqueConstraintCode = "23505";

    public AppException Translate(DbUpdateException exception)
    {
        var inner = exception.InnerException;
        
        var tableName = inner?.Data[TableNameKey]?.ToString();
        var isTransient = false;
        
        if (inner is Npgsql.PostgresException pgEx)
        {
            tableName = pgEx.TableName ?? tableName;

            isTransient = pgEx.SqlState is SerializationFailureCode or QueryCancelledCode;

            if (pgEx.SqlState == UniqueConstraintCode)
            {
                return tableName switch
                {
                    "Users" => new ConflictException(
                        ErrorCodes.UserEmailExists,
                        "User Email already exists",
                        "User Email already exists"
                    ),

                    "Tenants" => new ConflictException(
                        ErrorCodes.TenantNameExists,
                        "Workspace Name already exists",
                        "Workspace Name already exists"
                    ),

                    _ => new ConflictException(
                        ErrorCodes.DuplicateValueDetected,
                        $"Duplicate key violation for {tableName}",
                        $"Duplicate key violation for {tableName}"
                    ),
                };
            }
        }
        
        if (isTransient)
        {
            return new TransientServerException(
                "Error while creating user",
                new Dictionary<string, object> { ["reason"] = "Transient database failure" }
            );
        }

        return new InternalServerException(
            "Error while creating user",
            new Dictionary<string, object> { ["reason"] = "Unexpected database failure" }
        );
    }
}
