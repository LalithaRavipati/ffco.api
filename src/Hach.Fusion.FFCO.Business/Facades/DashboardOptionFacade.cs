﻿using System;
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
using Hach.Fusion.Data.Entities;
using Hach.Fusion.Data.Extensions;
using Hach.Fusion.Data.Mapping;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the DashboardOption repository. 
    /// </summary>    
    public class DashboardOptionFacade
        : FacadeWithCruModelsBase<DashboardOptionBaseDto, DashboardOptionBaseDto, DashboardOptionQueryDto, Guid>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="DashboardOptionFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing dashboard option entities.</param>
        /// <param name="validator">Validator for DTOs.</param>
        public DashboardOptionFacade(DataContext context, IFFValidator<DashboardOptionBaseDto> validator)
        {
            _context = context;
            _mapper = MappingManager.AutoMapper;
            ValidatorCreate = validator;
            ValidatorUpdate = validator;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of dashboard options from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<DashboardOptionQueryDto>> Get(ODataQueryOptions<DashboardOptionQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            try
            {
                var result = await Task.Run(() =>
                    _context.GetDashboardOptionsForUser(Guid.Parse(userId))
                    .Select(_mapper.Map<DashboardOption, DashboardOptionQueryDto>)
                    .AsQueryable())
                    .ConfigureAwait(false);

                return Query.Result(result);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Gets a single dashboard option from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the entity to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<DashboardOptionQueryDto>> Get(Guid id)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() => 
                _context.GetDashboardOptionsForUser(Guid.Parse(userId))
                .FirstOrDefault(x => x.Id == id))
                .ConfigureAwait(false);

            return result == null
                    ? Query.Error(EntityErrorCode.EntityNotFound)
                    : Query.Result(_mapper.Map<DashboardOption, DashboardOptionQueryDto>(result));
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a dashboard option.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create an entity.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the entity created.
        /// </returns>
        public override async Task<CommandResult<DashboardOptionQueryDto, Guid>> Create(DashboardOptionBaseDto dto)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<DashboardOptionQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<DashboardOptionQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid("Id"));

            var userTenants = _context.GetTenantsForUser(Guid.Parse(userId));

            if (dto.TenantId != Guid.Empty && !userTenants.Any(x => x.Id == dto.TenantId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));
            else if (_context.DashboardOptions.Any(x => x.TenantId == dto.TenantId))
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            if (validationResponse.IsInvalid)
                return Command.Error<DashboardOptionQueryDto>(validationResponse);

            var newEntity = new DashboardOption();

            _mapper.Map(dto, newEntity);

            newEntity.Id = Guid.NewGuid();
            newEntity.SetAuditFieldsOnCreate(userId);

            _context.DashboardOptions.Add(newEntity);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(_mapper.Map(newEntity, new DashboardOptionQueryDto()), newEntity.Id);
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a dashboard option.
        /// </summary>
        /// <param name="id">ID that identifies the entity to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<DashboardOptionQueryDto, Guid>> Delete(Guid id)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<DashboardOptionQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var userDashboardOptions = _context.GetDashboardOptionsForUser(Guid.Parse(userId));
            var entity = userDashboardOptions.SingleOrDefault(x => x.Id == id);

            if (entity == null)
                return Command.Error<DashboardOptionQueryDto>(EntityErrorCode.EntityNotFound);

            if (_context.Dashboards.Any(x => x.DashboardOptionId == id))
                return Command.Error<DashboardOptionQueryDto>(EntityErrorCode.EntityCouldNotBeDeleted);

            _context.DashboardOptions.Remove(entity);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<DashboardOptionQueryDto>();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a dashboard option using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the entity to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of entity properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<DashboardOptionBaseDto, Guid>> Update(Guid id, Delta<DashboardOptionBaseDto> delta)
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();
            if (userId == null)
                return Command.Error<DashboardOptionBaseDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            if (delta == null)
                return Command.Error<DashboardOptionBaseDto>(EntityErrorCode.EntityFormatIsInvalid);

            var userDashboardOptions = _context.GetDashboardOptionsForUser(Guid.Parse(userId));
            var entity = userDashboardOptions.SingleOrDefault(x => x.Id == id);

            if (entity == null)
                return Command.Error<DashboardOptionBaseDto>(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map(entity, new DashboardOptionBaseDto());
            delta.Patch(dto);

            var validationResponse = ValidatorUpdate.Validate(dto);

            var userTenants = _context.GetTenantsForUser(Guid.Parse(userId));

            if (dto.TenantId != Guid.Empty && !userTenants.Any(x => x.Id == dto.TenantId))
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist("TenantId"));
            else if (_context.DashboardOptions.Any(x => x.Id != id && x.TenantId == dto.TenantId))
                validationResponse.FFErrors.Add(EntityErrorCode.EntityAlreadyExists);

            // Including the original Id in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (dto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            if (validationResponse.IsInvalid)
                return Command.Error<DashboardOptionBaseDto>(validationResponse);

            _context.DashboardOptions.Attach(entity);
            _mapper.Map(dto, entity);
            entity.SetAuditFieldsOnUpdate(userId);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<DashboardOptionBaseDto>();
        }

        #endregion Update Method

        #region Not Implemented Methods

        public override Task<QueryResult<DashboardOptionQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<DashboardOptionQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<DashboardOptionQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods
    }
}
