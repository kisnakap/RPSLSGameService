namespace RPSLSGameService.Domain.Interfaces
{
    public interface IValidator<T>
    {
        bool Validate(T entity, out string error);
    }
}
