using FlagForge.Data.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace FlagForge.Data.Persistence.Interfaces;

public interface IDbExceptionTranslator
{
    AppException Translate(DbUpdateException exception);
}