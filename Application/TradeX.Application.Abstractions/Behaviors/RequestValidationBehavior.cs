using TradeX.Application.Abstractions.Enums;
using TradeX.Application.Abstractions.Factories;
using TradeX.Application.Abstractions.Interfaces;
using TradeX.Application.Abstractions.Models;
using FluentValidation;
using MediatR;

namespace TradeX.Application.Abstractions.Behaviors;

public class RequestValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IStandardResponse
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken) 
    {
        var context = new ValidationContext<TRequest>(request);

        if (_validators == null)
        {
            throw new NullReferenceException("IEnumerable<IValidator<TRequest>> cannot be null. Check Dependency Injection.");
        }

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        TResponse res;

        if (failures.Count != 0)
        {
            var propertyNames = failures
               .Select(e => e.PropertyName)
               .Distinct();

            var failureMessages = new Dictionary<string, string[]>();

            foreach (var propertyName in propertyNames)
            {
                var propertyFailures = failures
                    .Where(e => e.PropertyName == propertyName)
                    .Select(e => e.ErrorMessage)
                    .ToArray();

                failureMessages.Add(propertyName.TrimStart("Data.".ToCharArray()), propertyFailures);
            }
            string requestName = typeof(TRequest).Name;

            
            res = StandardResponseFactory.CreateError<TResponse>(
                OperationResult.BadRequest, 
                "Validation Error",
                new ValidationErrorResponseModel()
                {
                    ValidationErrors = failureMessages
                });
        }
        else
        {
            res = await next();
        }

        return res;
    }
}