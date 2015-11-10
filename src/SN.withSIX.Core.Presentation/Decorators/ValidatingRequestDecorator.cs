// <copyright company="SIX Networks GmbH" file="ValidatingRequestDecorator.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using ShortBus;

namespace SN.withSIX.Core.Presentation.Decorators
{
    public abstract class BaseValidator
    {
        readonly DataAnnotationsValidator.DataAnnotationsValidator _validator =
            new DataAnnotationsValidator.DataAnnotationsValidator();

        [DebuggerStepThrough]
        protected void Validate(object o) {
            //var validationContext = new ValidationContext(query, this.provider, null);
            //Validator.ValidateObject(o, new ValidationContext(query), true); // , validationContext
            _validator.ValidateObject(o); // , validationContext
        }
    }

    public class MediatorValidationDecorator : BaseValidator, IMediator
    {
        readonly IMediator _mediator;

        public MediatorValidationDecorator(IMediator mediator) {
            Contract.Requires<ArgumentNullException>(mediator != null);
            _mediator = mediator;
        }

        public TResponseData Request<TResponseData>(IRequest<TResponseData> request) {
            Validate(request);
            return _mediator.Request(request);
        }

        public Task<TResponseData> RequestAsync<TResponseData>(IAsyncRequest<TResponseData> request) {
            Validate(request);
            return _mediator.RequestAsync(request);
        }

        public void Notify<TNotification>(TNotification notification) {
            //Validate(notification);
            _mediator.Notify(notification);
        }

        public Task NotifyAsync<TNotification>(TNotification notification) {
            //Validate(notification);
            return _mediator.NotifyAsync(notification);
        }
    }
}