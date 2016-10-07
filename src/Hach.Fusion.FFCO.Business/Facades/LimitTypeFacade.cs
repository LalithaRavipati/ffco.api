using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
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
using Hach.Fusion.FFCO.Dtos.Dashboards;
using Hach.Fusion.FFCO.Dtos.LimitTypes;
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Extensions;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the LimitType repository. 
    /// </summary>    
    public class LimitTypeFacade
        : FacadeWithCruModelsBase<LimitTypeCommandDto, LimitTypeCommandDto, LimitTypeQueryDto, Guid>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="LimitTypeFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing dashboard option entities.</param>
        /// <param name="validator">Validator for DTOs.</param>
        public LimitTypeFacade(DataContext context, IFFValidator<LimitTypeCommandDto> validator)
        {
            _context = context;
            _mapper = MappingManager.AutoMapper;
            ValidatorCreate = validator;
            ValidatorUpdate = validator;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of limit types from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<LimitTypeQueryDto>> Get(ODataQueryOptions<LimitTypeQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var results = await Task.Run(() => _context.LimitTypes
                .Select(_mapper.Map<LimitType, LimitTypeQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single limit type from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<LimitTypeQueryDto>> Get(Guid id)
        {
            var result = await Task.Run(() => _context.LimitTypes
                .FirstOrDefault(x => x.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map<LimitType, LimitTypeQueryDto>(result);

            return Query.Result(dto);
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a limit type.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create an entity.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the entity created.
        /// </returns>
        public override async Task<CommandResult<LimitTypeQueryDto, Guid>> Create(LimitTypeCommandDto dto)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<LimitTypeQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<LimitTypeQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid("Id"));

            if (_context.LimitTypes.Any(x => x.I18NKeyName == dto.I18NKeyName))
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            if (validationResponse.IsInvalid)
                return Command.Error<LimitTypeQueryDto>(validationResponse);

            var newEntity = new LimitType();

            _mapper.Map(dto, newEntity);

            newEntity.Id = Guid.NewGuid();
            newEntity.SetAuditFieldsOnCreate(userId);

            _context.LimitTypes.Add(newEntity);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(_mapper.Map(newEntity, new LimitTypeQueryDto()), newEntity.Id);
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a limit type.
        /// </summary>
        /// <param name="id">ID that identifies the entity to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LimitTypeQueryDto, Guid>> Delete(Guid id)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<LimitTypeQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var entity = await _context.LimitTypes
              .SingleOrDefaultAsync(x => x.Id == id)
              .ConfigureAwait(false);

            if (entity == null)
                return Command.Error<LimitTypeQueryDto>(EntityErrorCode.EntityNotFound);

            _context.LimitTypes.Attach(entity);
            entity.SetAuditFieldsOnUpdate(userId);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            _context.LimitTypes.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LimitTypeQueryDto>();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a limit type using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the entity to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of entity properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LimitTypeCommandDto, Guid>> Update(Guid id, Delta<LimitTypeCommandDto> delta)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<LimitTypeCommandDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            if (delta == null)
                return Command.Error<LimitTypeCommandDto>(EntityErrorCode.EntityFormatIsInvalid);

            var entity = await _context.LimitTypes
                .SingleOrDefaultAsync(x => x.Id == id)
                .ConfigureAwait(false);

            if (entity == null)
                return Command.Error<LimitTypeCommandDto>(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map(entity, new LimitTypeCommandDto());
            delta.Patch(dto);

            var validationResponse = ValidatorUpdate.Validate(dto);

            if (_context.LimitTypes.Any(x => x.Id != id && x.I18NKeyName == dto.I18NKeyName))
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityPropertyDuplicateNotAllowed(nameof(LocationType.I18NKeyName)));

            // Including the original Id in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (dto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            if (validationResponse.IsInvalid)
                return Command.Error<LimitTypeCommandDto>(validationResponse);

            _context.LimitTypes.Attach(entity);
            _mapper.Map(dto, entity);
            entity.SetAuditFieldsOnUpdate(userId);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LimitTypeCommandDto>();
        }

        #endregion Update Method

        #region Not Implemented Methods

        public override Task<QueryResult<LimitTypeQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LimitTypeQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LimitTypeQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
