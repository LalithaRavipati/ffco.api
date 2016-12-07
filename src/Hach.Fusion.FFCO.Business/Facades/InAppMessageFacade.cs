using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.OData;
using System.Web.OData.Query;
using AutoMapper;
using Hach.Fusion.Core.Api.Security;
using Hach.Fusion.Core.Business.Facades;
using Hach.Fusion.Core.Business.Results;
using Hach.Fusion.Core.Business.Validation;
using Hach.Fusion.FFCO.Business.Database;
using Hach.Fusion.FFCO.Business.Extensions;
using Hach.Fusion.FFCO.Core.Dtos;
using Hach.Fusion.FFCO.Core.Entities;
using System.Threading;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the InAppMessage repository. 
    /// </summary>    
    public class InAppMessageFacade
        : FacadeWithCruModelsBase<InAppMessageCommandDto, InAppMessageCommandDto, InAppMessageQueryDto, Guid>,
        IInAppMessageFacade
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="InAppMessageFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing InAppMessage entities.</param>
        /// <param name="validator">Validator for InAppMessage DTOs.</param>
        public InAppMessageFacade(DataContext context, IFFValidator<InAppMessageCommandDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of InAppMessages from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of InAppMessage DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<InAppMessageQueryDto>> Get(ODataQueryOptions<InAppMessageQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var results = await Task.Run(() => _context.GetInAppMessagesForUser(uid.Value)
                .Select(_mapper.Map<InAppMessage, InAppMessageQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single InAppMessage from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the InAppMessage to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the InAppMessage DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<InAppMessageQueryDto>> Get(Guid id)
        {
            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() => _context.GetInAppMessagesForUser(uid.Value)
                .FirstOrDefault(msg => msg.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<InAppMessage, InAppMessageQueryDto>(result);

            return Query.Result(dto);
        }


        public async Task<QueryResult<InAppMessageQueryDto>> GetByUserId(Guid userId
            , ODataQueryOptions<InAppMessageQueryDto> options)
        {
            options.Validate(ValidationSettings);

            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            // Check if the calling User shares a Tenant with the UserId passed in
            if (!_context.GetTenantsForUser(uid.Value).Any(u => u.Id == userId))
            {
                return Query.Error(EntityErrorCode.EntityNotFound);
            }

            var results = 
                await Task.Run(() => 
                    _context.GetInAppMessagesForUser(userId)
                    .Select(_mapper.Map<InAppMessage, InAppMessageQueryDto>)
                    .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }


        #endregion Get Methods

        #region Update Methods

        public override Task<CommandResult<InAppMessageCommandDto, Guid>> Update(Guid id, Delta<InAppMessageCommandDto> delta)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Utility Methods

        private Guid? GetCurrentUser()
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            if (userId == null)
                return null;
            else
                return Guid.Parse(userId);
        }

        #endregion

        #region Not Implemented Methods

        public override Task<QueryResult<InAppMessageQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<InAppMessageQueryDto, Guid>> Create(InAppMessageCommandDto dto)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<InAppMessageQueryDto, Guid>> Delete(Guid id)
        {
            throw new NotImplementedException();
        }


        public override Task<CommandResult<InAppMessageQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<InAppMessageQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
