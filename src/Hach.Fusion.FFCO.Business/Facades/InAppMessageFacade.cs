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
using Hach.Fusion.FFCO.Core.Extensions;

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

            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        public async Task<QueryResult<InAppMessageQueryDto>> GetByUserId(Guid userId
            , ODataQueryOptions<InAppMessageQueryDto> options)
        {
            options.Validate(ValidationSettings);

            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));


            var tenants = _context.GetTenantsForUser(uid.Value);
            var shareTenant = tenants.Any(t => t.Users.Any(u=> u.Id == userId));

            // Check if the calling User shares a Tenant with the UserId passed in
            if (!shareTenant)
                return Query.Error(EntityErrorCode.EntityNotFound);

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

        public override async Task<CommandResult<InAppMessageCommandDto, Guid>> Update(Guid id, Delta<InAppMessageCommandDto> delta)
        {
            // Check user has proper access to update the message
            var uid = GetCurrentUser();

            if (!uid.HasValue)
                return Command.Error<InAppMessageCommandDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var entity = _context.InAppMessages.SingleOrDefault(msg => msg.Id == id);
            if (entity == null)
                return Command.Error<InAppMessageCommandDto>(EntityErrorCode.EntityNotFound);

            // Check if the calling User shares a Tenant with the UserId passed in
            var tenants = _context.GetTenantsForUser(uid.Value);
            var shareTenant = tenants.Any(t => t.Users.Any(u => u.Id == entity.UserId));
            if (!shareTenant)
                return Command.Error<InAppMessageCommandDto>(ValidationErrorCode.ForeignKeyValueDoesNotExist("UserId"));

            var dto = _mapper.Map(entity, new InAppMessageCommandDto());

            delta.Patch(dto);

            var validationResponse = ValidatorUpdate.Validate(dto);

            if (validationResponse.IsInvalid)
                return Command.Error<InAppMessageCommandDto>(validationResponse);

            // Apply the update
            _context.InAppMessages.Attach(entity);
            _mapper.Map(dto, entity);
            entity.SetAuditFieldsOnUpdate(uid.Value);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<InAppMessageCommandDto>();
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

        public override Task<QueryResult<InAppMessageQueryDto>> Get(Guid id)
        {
            throw new NotImplementedException();
        }

        public override Task<QueryResult<InAppMessageQueryDto>> Get(ODataQueryOptions<InAppMessageQueryDto> queryOptions)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
