using System;
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
using Hach.Fusion.Data.Database;
using Hach.Fusion.Data.Dtos;
using Hach.Fusion.Data.Extensions;
using Hach.Fusion.Data.Entities;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Dashboard repository. 
    /// </summary>    
    public class DashboardFacade
        : FacadeWithCruModelsBase<DashboardCommandDto, DashboardCommandDto, DashboardQueryDto, Guid>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="DashboardFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing dashboard type entities.</param>
        /// <param name="validator">Validator for DTOs.</param>
        public DashboardFacade(DataContext context, IFFValidator<DashboardCommandDto> validator)
        {
            _context = context;
            _mapper = MappingManager.AutoMapper;
            ValidatorCreate = validator;
            ValidatorUpdate = validator;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of dashboards from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<DashboardQueryDto>> Get(ODataQueryOptions<DashboardQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() => 
                _context.GetDashboardsForUser(Guid.Parse(userId))
                .Select(_mapper.Map<Dashboard, DashboardQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(result);
        }

        /// <summary>
        /// Gets a single dashboard from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<DashboardQueryDto>> Get(Guid id)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() => 
                _context.GetDashboardsForUser(Guid.Parse(userId))
                .FirstOrDefault(x => x.Id == id))
                .ConfigureAwait(false);

            return result == null 
                ? Query.Error(EntityErrorCode.EntityNotFound) 
                : Query.Result(_mapper.Map<Dashboard, DashboardQueryDto>(result));
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a dashboard.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create an entity.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the entity created.
        /// </returns>
        public override async Task<CommandResult<DashboardQueryDto, Guid>> Create(DashboardCommandDto dto)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<DashboardQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var userIdGuid = Guid.Parse(userId);

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<DashboardQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid("Id"));

            var userTenants = _context.GetTenantsForUser(userIdGuid);

            if (dto.TenantId != Guid.Empty && !userTenants.Any(x => x.Id == dto.TenantId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));

            if (_context.Dashboards.Any(x => x.OwnerUserId == userIdGuid && x.Name == dto.Name))
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            var userDashboardOptions = _context.GetDashboardOptionsForUser(userIdGuid);

            if (dto.DashboardOptionId != Guid.Empty && !userDashboardOptions.Any(x => x.Id == dto.DashboardOptionId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("DashboardOptionId"));

            if (validationResponse.IsInvalid)
                return Command.Error<DashboardQueryDto>(validationResponse);

            var newEntity = new Dashboard();

            _mapper.Map(dto, newEntity);

            newEntity.Id = Guid.NewGuid();
            newEntity.SetAuditFieldsOnCreate(userId);
            newEntity.OwnerUserId = userIdGuid;

            _context.Dashboards.Add(newEntity);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(_mapper.Map(newEntity, new DashboardQueryDto()), newEntity.Id);
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a dashboard.
        /// </summary>
        /// <param name="id">ID that identifies the entity to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<DashboardQueryDto, Guid>> Delete(Guid id)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<DashboardQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var userDashboards = _context.GetDashboardsForUser(Guid.Parse(userId));
            var entity = userDashboards.SingleOrDefault(x => x.Id == id);

            if (entity == null)
                return Command.Error<DashboardQueryDto>(EntityErrorCode.EntityNotFound);

            var userIdGuid = Guid.Parse(userId);
            if (entity.OwnerUserId != userIdGuid)
                return Command.Error<DashboardQueryDto>(EntityErrorCode.EntityCouldNotBeDeleted);

            _context.Dashboards.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<DashboardQueryDto>();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a dashboard using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the entity to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of entity properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<DashboardCommandDto, Guid>> Update(Guid id, Delta<DashboardCommandDto> delta)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<DashboardCommandDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var userIdGuid = Guid.Parse(userId);

            if (delta == null)
                return Command.Error<DashboardCommandDto>(EntityErrorCode.EntityFormatIsInvalid);

            var userDashboards = _context.GetDashboardsForUser(Guid.Parse(userId));
            var entity = userDashboards.SingleOrDefault(x => x.Id == id);

            if (entity == null)
                return Command.Error<DashboardCommandDto>(EntityErrorCode.EntityNotFound);

            // Only the dashboard creator can modify the dashboard.
            if (entity.OwnerUserId != userIdGuid)
                return Command.Error<DashboardCommandDto>(EntityErrorCode.PermissionActivityInvalid);

            var dto = _mapper.Map(entity, new DashboardCommandDto());
            delta.Patch(dto);

            var validationResponse = ValidatorUpdate.Validate(dto);

            if (_context.Dashboards.Any(x => x.Id != id && x.OwnerUserId == userIdGuid && x.Name == dto.Name))
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            var userTenants = _context.GetTenantsForUser(Guid.Parse(userId));

            if (dto.TenantId != Guid.Empty && !userTenants.Any(x => x.Id == dto.TenantId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));

            var userDashboardOptions = _context.GetDashboardOptionsForUser(Guid.Parse(userId));

            if (dto.DashboardOptionId != Guid.Empty && !userDashboardOptions.Any(x => x.Id == dto.DashboardOptionId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("DashboardOptionId"));

            // Including the original Id in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (dto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            if (validationResponse.IsInvalid)
                return Command.Error<DashboardCommandDto>(validationResponse);

            _context.Dashboards.Attach(entity);
            _mapper.Map(dto, entity);
            entity.SetAuditFieldsOnUpdate(userId);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<DashboardCommandDto>();
        }

        #endregion Update Method

        #region Not Implemented Methods

        public override Task<QueryResult<DashboardQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<DashboardQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<DashboardQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
