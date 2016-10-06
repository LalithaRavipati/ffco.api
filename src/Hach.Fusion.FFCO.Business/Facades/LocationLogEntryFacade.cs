using Hach.Fusion.FFCO.Dtos;
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
using Hach.Fusion.FFCO.Entities;
using Hach.Fusion.FFCO.Entities.Extensions;

namespace Hach.Fusion.FFCO.Business.Facades
{
    /// <summary>
    /// Facade for managing the Location Log Entry repository. 
    /// </summary>    
    public class LocationLogEntryFacade
        : FacadeWithCruModelsBase<LocationLogEntryCommandDto, LocationLogEntryCommandDto, LocationLogEntryQueryDto, Guid>
    {
        private readonly DataContext _context;

        private readonly IMapper _mapper;

        /// <summary>
        /// Constructor for the <see cref="LocationLogEntryFacade"/> class taking a database context
        /// and validator argument.
        /// </summary>
        /// <param name="context">Database context containing location type entities.</param>
        /// <param name="validator">Validator for location DTOs.</param>
        public LocationLogEntryFacade(DataContext context, IFFValidator<LocationLogEntryCommandDto> validator)
        {
            _context = context;

            ValidatorCreate = validator;
            ValidatorUpdate = validator;

            _mapper = MappingManager.AutoMapper;
        }

        #region Get Methods

        /// <summary>
        /// Gets a list of Location Log Entries from the data store.
        /// </summary>
        /// <param name="queryOptions">OData query options.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the list of Location Log Entry DTOs retrieved.
        /// </returns>
        public override async Task<QueryResult<LocationLogEntryQueryDto>> Get(ODataQueryOptions<LocationLogEntryQueryDto> queryOptions)
        {
            queryOptions.Validate(ValidationSettings);

            // User ID should always be available, but if not ...
            var userId = GetCurrentUser();
            if (!userId.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var results = await Task.Run(() => GetLocationLogEntriesForUser(userId.Value)
                .Select(_mapper.Map<LocationLogEntry, LocationLogEntryQueryDto>)
                .AsQueryable())
                .ConfigureAwait(false);

            return Query.Result(results);
        }

        /// <summary>
        /// Gets a single Location Log Entry from the data store.
        /// </summary>
        /// <param name="id">ID that uniquely identifies the Location Log Entry to be retrieved.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result includes the Location Log Entry DTO retrieved.
        /// </returns>
        public override async Task<QueryResult<LocationLogEntryQueryDto>> Get(Guid id)
        {
            // User ID should always be available, but if not ...
            var userId = GetCurrentUser();
            if (!userId.HasValue)
                return Query.Error(GeneralErrorCodes.TokenInvalid("UserId"));

            var result = await Task.Run(() => GetLocationLogEntriesForUser(userId.Value)
                .FirstOrDefault(l => l.Id == id))
                .ConfigureAwait(false);

            if (result == null)
                return Query.Error(EntityErrorCode.EntityNotFound);

            var locationLogEntryQueryDto = _mapper.Map<LocationLogEntry, LocationLogEntryQueryDto>(result);

            return Query.Result(locationLogEntryQueryDto);
        }

        #endregion Get Methods

        #region Create Method

        /// <summary>
        /// Creates a Location Log Entry.
        /// </summary>
        /// <param name="dto">Data Transfer Object (DTO) used to create a Location Log Entry.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// If successful, the task result contains the DTO associated with the Location Log Entry.
        /// </returns>
        public override async Task<CommandResult<LocationLogEntryQueryDto, Guid>> Create(LocationLogEntryCommandDto dto)
        {
            // User ID should always be available, but if not ...
            var userId = GetCurrentUser();
            if (!userId.HasValue)
                return Command.Error<LocationLogEntryQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var validationResponse = ValidatorCreate.Validate(dto);

            if (dto == null)
                return Command.Error<LocationLogEntryQueryDto>(validationResponse);

            if (dto.Id != Guid.Empty)
                validationResponse.FFErrors.Add(ValidationErrorCode.PropertyIsInvalid(nameof(LocationLogEntry.Id)));

            var locationExists = await DoesLocationExist(dto.LocationId);
            if (!locationExists)
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist(nameof(LocationLogEntry.LocationId)));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationLogEntryQueryDto>(validationResponse);

            var locationLogEntry = new LocationLogEntry
            {
                Id = Guid.NewGuid()
            };

            _mapper.Map(dto, locationLogEntry);

            locationLogEntry.SetAuditFieldsOnCreate(userId.Value);
            
            _context.LocationLogEntries.Add(locationLogEntry);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.Created(_mapper.Map(locationLogEntry, new LocationLogEntryQueryDto()), locationLogEntry.Id);
        }

        #endregion Create Method

        #region Delete Method

        /// <summary>
        /// Deletes a Location Log Entry.
        /// </summary>
        /// <param name="id">ID that identifies the Location Log Entry to be deleted.</param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LocationLogEntryQueryDto, Guid>> Delete(Guid id)
        {
            // User ID should always be available, but if not ...
            var userId = GetCurrentUser();
            if (!userId.HasValue)
                return Command.Error<LocationLogEntryQueryDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            var locationLogEntry = await GetLocationLogEntriesForUser(userId.Value)                        
              .SingleOrDefaultAsync(l => l.Id == id)
              .ConfigureAwait(false);

            if (locationLogEntry == null)
                return Command.Error<LocationLogEntryQueryDto>(EntityErrorCode.EntityNotFound);

            _context.LocationLogEntries.Remove(locationLogEntry);
            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationLogEntryQueryDto>();
        }

        #endregion Delete Method

        #region Update Method

        /// <summary>
        /// Updates a Location Log Entry using a <see cref="Delta"/> object.
        /// </summary>
        /// <param name="id">ID of the Location Log Entry to be updated.</param>
        /// <param name="delta">
        /// Delta containing a list of Location Log Entry properties.  Web Api does the magic of converting the JSON to 
        /// a delta.
        /// </param>
        /// <returns>
        /// An asynchronous task result containing information needed to create an API response message.
        /// </returns>
        public override async Task<CommandResult<LocationLogEntryCommandDto, Guid>> Update(Guid id, Delta<LocationLogEntryCommandDto> delta)
        {
            // User ID should always be available, but if not ...
            var userId = GetCurrentUser();
            if (!userId.HasValue)
                return Command.Error<LocationLogEntryCommandDto>(GeneralErrorCodes.TokenInvalid("UserId"));

            if (delta == null)
                return Command.Error<LocationLogEntryCommandDto>(EntityErrorCode.EntityFormatIsInvalid);

            var locationLogEntry = await GetLocationLogEntriesForUser(userId.Value)
                .SingleOrDefaultAsync(l => l.Id == id)
                .ConfigureAwait(false);

            if (locationLogEntry == null)
                return Command.Error<LocationLogEntryCommandDto>(EntityErrorCode.EntityNotFound);

            var dto = _mapper.Map(locationLogEntry, new LocationLogEntryCommandDto());
            delta.Patch(dto);

            var validationResponse = ValidatorUpdate.Validate(dto);

            // Including the original ID in the Patch request will not return an error but attempting to change the Id is not allowed.
            if (dto.Id != id)
                validationResponse.FFErrors.Add(ValidationErrorCode.EntityIDUpdateNotAllowed("Id"));

            var locationExists = await DoesLocationExist(dto.LocationId);
            if (!locationExists)
                validationResponse.FFErrors.Add(ValidationErrorCode.ForeignKeyValueDoesNotExist(nameof(LocationLogEntry.LocationId)));

            if (validationResponse.IsInvalid)
                return Command.Error<LocationLogEntryCommandDto>(validationResponse);

            _context.LocationLogEntries.Attach(locationLogEntry);
            _mapper.Map(dto, locationLogEntry);

            locationLogEntry.SetAuditFieldsOnUpdate(userId.Value);

            await _context.SaveChangesAsync().ConfigureAwait(false);

            return Command.NoContent<LocationLogEntryCommandDto>();
        }

        #endregion Update Method

        #region Not Implemented Methods

        public override Task<QueryResult<LocationLogEntryQueryDto>> GetProperty(Guid id, string propertyName)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationLogEntryQueryDto, Guid>> CreateReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        public override Task<CommandResult<LocationLogEntryQueryDto, Guid>> DeleteReference(Guid id, string navigationProperty, object referenceId)
        {
            throw new NotImplementedException();
        }

        #endregion Not Implemented Methods

        #region Facade Helper Methods

        private async Task<bool> DoesLocationExist(Guid locationId)
        {
            var userId = GetCurrentUser();

            return await _context.ProductOfferingTenantLocations.AnyAsync(
                potl => potl.LocationId == locationId && potl.Tenant.Users.Any(u => u.Id == userId));
        }

        /// <summary>
        /// Gets the GUID User ID for the current user.
        /// </summary>
        /// <returns>The nullable GUID for the current user.</returns>
        private Guid? GetCurrentUser()
        {
            var userId = Thread.CurrentPrincipal == null ? null : Thread.CurrentPrincipal.GetUserIdFromPrincipal();

            return userId == null ? null : new Guid?(Guid.Parse(userId));
        }

        /// <summary>
        /// Gets a list of Location Log Entities for which the specified user is authorized.
        /// </summary>
        /// <returns>
        /// A queryable list of Location Log Entities for which the specified user is authorized.
        /// </returns>
        private IQueryable<LocationLogEntry> GetLocationLogEntriesForUser(Guid userId)
        {
            var result =
                _context.LocationLogEntries.Join(_context.ProductOfferingTenantLocations, lle => lle.LocationId,
                    potl => potl.LocationId, (lle, potl) => new {lle, potl})
                    .Where(@t => @t.potl.Tenant.Users.Any(x => x.Id == userId))
                    .Select(@t => @t.lle).Distinct();

            return result;
        }

        #endregion Facade Helper Methods
    }
}
